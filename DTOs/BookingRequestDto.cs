using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs
{
    public class BookingRequestDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string CustomerEmail { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string CustomerPhone { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        // این فیلد برای Idempotency از هدر دریافت می‌شود، اما می‌توان در بدنه هم آورد
        public string IdempotencyKey { get; set; }
    }
}