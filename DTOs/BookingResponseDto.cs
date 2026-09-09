namespace HotelBookingAPI.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IdempotencyKey { get; set; }
    }
}