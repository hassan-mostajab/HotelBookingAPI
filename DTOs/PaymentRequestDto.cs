using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs
{
    public class PaymentRequestDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Currency { get; set; } = "USD";

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }
    }
}