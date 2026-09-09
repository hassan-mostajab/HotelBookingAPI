using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingAPI.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Currency { get; set; } = "USD";

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } // "CreditCard", "PayPal", ...

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string TransactionId { get; set; } // از درگاه پرداخت

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
    }
}