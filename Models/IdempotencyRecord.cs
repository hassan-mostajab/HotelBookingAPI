using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Models
{
    public class IdempotencyRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } // کلید یکتای دریافتی

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public string ResponseBody { get; set; } // ذخیره پاسخ قبلی برای بازگشت

        public int StatusCode { get; set; } // کد وضعیت پاسخ قبلی
    }
}