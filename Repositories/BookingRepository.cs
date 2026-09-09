using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Booking> GetBookingWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Room)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> IsIdempotencyKeyExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return await _dbSet.AnyAsync(b => b.IdempotencyKey == key);
        }
    }
}