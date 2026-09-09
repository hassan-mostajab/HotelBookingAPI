namespace HotelBookingAPI.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
    }
}