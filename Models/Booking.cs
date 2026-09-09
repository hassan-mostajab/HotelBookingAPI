using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingAPI.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

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

        public decimal TotalPrice { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // کلید یکتا برای جلوگیری از ثبت دوگانه (Idempotency)
        [MaxLength(100)]
        public string IdempotencyKey { get; set; }

        // رابطه با Room
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        // رابطه با Payment
        public virtual Payment Payment { get; set; }
    }
}