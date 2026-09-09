using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Room> GetAvailableRoomAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // واکشی اتاق با قفل (برای جلوگیری از تغییر همزمان)
            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == roomId && r.IsAvailable);

            if (room == null)
                return null;

            // بررسی تداخل تاریخ‌ها
            bool isAvailable = !room.Bookings.Any(b =>
                b.IsConfirmed &&
                ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                 (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                 (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));

            return isAvailable ? room : null;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = await GetAvailableRoomAsync(roomId, checkIn, checkOut);
            return room != null;
        }
    }
}