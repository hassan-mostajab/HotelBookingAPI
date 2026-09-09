using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<Room> GetAvailableRoomAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
    }
}