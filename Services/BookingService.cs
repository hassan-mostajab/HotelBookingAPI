using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IRoomRepository roomRepository,
            IIdempotencyService idempotencyService,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string idempotencyKey)
        {
            // ۱. بررسی Idempotency
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existingBooking = await _bookingRepository
                    .FindAsync(b => b.IdempotencyKey == idempotencyKey);

                if (existingBooking.Any())
                {
                    _logger.LogInformation("درخواست تکراری با کلید {Key} دریافت شد.", idempotencyKey);
                    return MapToResponseDto(existingBooking.First());
                }
            }

            // ۲. واکشی اتاق با قفل جهت بررسی هم‌زمانی
            var room = await _roomRepository.GetAvailableRoomAsync(
                request.RoomId, request.CheckInDate, request.CheckOutDate);

            if (room == null)
                throw new InvalidOperationException("اتاق مورد نظر در تاریخ‌های انتخاب شده در دسترس نیست.");

            // ۳. محاسبه قیمت کل
            var nights = (request.CheckOutDate - request.CheckInDate).Days;
            if (nights <= 0)
                throw new ArgumentException("تاریخ خروج باید بعد از تاریخ ورود باشد.");

            var totalPrice = nights * room.PricePerNight;

            // ۴. ساخت رزرو جدید
            var booking = new Booking
            {
                RoomId = room.Id,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = totalPrice,
                IsConfirmed = false,
                IdempotencyKey = idempotencyKey,
                CreatedAt = DateTime.UtcNow
            };

            // ۵. ذخیره در دیتابیس (با مدیریت هم‌زمانی)
            try
            {
                await _bookingRepository.AddAsync(booking);
                await _bookingRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "خطای هم‌زمانی در زمان ثبت رزرو.");
                throw new DbUpdateConcurrencyException("اتاق توسط کاربر دیگری در حال رزرو است. لطفاً دوباره تلاش کنید.");
            }

            // ۶. به‌روزرسانی وضعیت اتاق (غیرفعال)
            room.IsAvailable = false;
            _roomRepository.Update(room);
            await _roomRepository.SaveChangesAsync();

            _logger.LogInformation("رزرو با شناسه {Id} برای اتاق {RoomId} ایجاد شد.", booking.Id, room.Id);

            // ۷. ذخیره Idempotency Record (اختیاری)
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var responseDto = MapToResponseDto(booking);
                await _idempotencyService.StoreResponseAsync(idempotencyKey, responseDto);
            }

            return MapToResponseDto(booking);
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(id);
            if (booking == null)
                throw new KeyNotFoundException($"رزرو با شناسه {id} یافت نشد.");

            return MapToResponseDto(booking);
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new KeyNotFoundException($"رزرو با شناسه {id} یافت نشد.");

            // فقط رزروهای تأیید نشده قابل لغو هستند
            if (booking.IsConfirmed)
                throw new InvalidOperationException("امکان لغو رزرو تأیید شده وجود ندارد.");

            // آزادسازی اتاق
            var room = await _roomRepository.GetByIdAsync(booking.RoomId);
            if (room != null)
            {
                room.IsAvailable = true;
                _roomRepository.Update(room);
            }

            _bookingRepository.Delete(booking);
            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation("رزرو با شناسه {Id} لغو شد.", id);
            return true;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByRoomAsync(int roomId)
        {
            var bookings = await _bookingRepository
                .FindAsync(b => b.RoomId == roomId);

            return bookings.Select(MapToResponseDto);
        }

        private BookingResponseDto MapToResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber ?? "",
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                IsConfirmed = booking.IsConfirmed,
                CreatedAt = booking.CreatedAt,
                IdempotencyKey = booking.IdempotencyKey
            };
        }
    }
}