using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string idempotencyKey);
        Task<BookingResponseDto> GetBookingByIdAsync(int id);
        Task<bool> CancelBookingAsync(int id);
        Task<IEnumerable<BookingResponseDto>> GetBookingsByRoomAsync(int roomId);
    }
}