using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IBookingRepository bookingRepository,
            IRepository<Payment> paymentRepository,
            IIdempotencyService idempotencyService,
            ILogger<PaymentService> logger)
        {
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        public async Task<Payment> ProcessPaymentAsync(PaymentRequestDto request)
        {
            // ۱. بررسی Idempotency (با کلید درخواست)
            var idempotencyKey = $"{request.BookingId}_{request.Amount}_{request.TransactionId}";
            var existingRecord = await _idempotencyService.GetResponseAsync<Payment>(idempotencyKey);
            if (existingRecord != null)
            {
                _logger.LogInformation("درخواست پرداخت تکراری برای کلید {Key}", idempotencyKey);
                return existingRecord;
            }

            // ۲. واکشی رزرو
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                throw new KeyNotFoundException($"رزرو با شناسه {request.BookingId} یافت نشد.");

            // ۳. بررسی تطابق مبلغ
            if (request.Amount != booking.TotalPrice)
                throw new ArgumentException("مبلغ پرداختی با مبلغ رزرو همخوانی ندارد.");

            // ۴. بررسی اینکه قبلاً پرداختی ثبت نشده باشد
            var existingPayment = await _paymentRepository
                .FindAsync(p => p.BookingId == request.BookingId && p.Status == "Completed");

            if (existingPayment.Any())
                throw new InvalidOperationException("پرداخت قبلاً برای این رزرو انجام شده است.");

            // ۵. ساخت پرداخت
            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                Status = "Completed", // در عمل باید از درگاه تأیید شود
                TransactionId = request.TransactionId ?? Guid.NewGuid().ToString(),
                PaymentDate = DateTime.UtcNow
            };

            // ۶. ذخیره در دیتابیس (با تراکنش)
            await using var transaction = await _paymentRepository._context.Database.BeginTransactionAsync();
            try
            {
                await _paymentRepository.AddAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                // به‌روزرسانی وضعیت رزرو
                booking.IsConfirmed = true;
                _bookingRepository.Update(booking);
                await _bookingRepository.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "خطای هم‌زمانی در زمان ثبت پرداخت.");
                throw new DbUpdateConcurrencyException("خطا در ثبت پرداخت. لطفاً دوباره تلاش کنید.");
            }

            // ۷. ذخیره Idempotency Record
            await _idempotencyService.StoreResponseAsync(idempotencyKey, payment);

            _logger.LogInformation("پرداخت با شناسه {Id} برای رزرو {BookingId} ثبت شد.", payment.Id, booking.Id);
            return payment;
        }

        public async Task<Payment> GetPaymentByBookingIdAsync(int bookingId)
        {
            var payments = await _paymentRepository.FindAsync(p => p.BookingId == bookingId);
            return payments.FirstOrDefault();
        }
    }
}