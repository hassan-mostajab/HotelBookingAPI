using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking> GetBookingWithDetailsAsync(int id);
        Task<bool> IsIdempotencyKeyExistsAsync(string key);
    }
}