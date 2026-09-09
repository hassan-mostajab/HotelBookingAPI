using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services
{
    public interface IIdempotencyService
    {
        Task StoreResponseAsync<T>(string key, T response) where T : class;
        Task<T> GetResponseAsync<T>(string key) where T : class;
        Task CleanExpiredRecordsAsync();
    }
}