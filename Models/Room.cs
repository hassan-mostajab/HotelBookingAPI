using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingAPI.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } // مثلاً "Single", "Double", "Suite"

        [Required]
        public decimal PricePerNight { get; set; }

        [Required]
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; } = true;

        // عملیات هم‌زمانی: این فیلد به عنوان RowVersion در MySQL استفاده می‌شود
        [Timestamp]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; }

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}