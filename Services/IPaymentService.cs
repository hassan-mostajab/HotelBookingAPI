using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(PaymentRequestDto request);
        Task<Payment> GetPaymentByBookingIdAsync(int bookingId);
    }
}