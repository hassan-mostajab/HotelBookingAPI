using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;
using HotelBookingAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelBookingAPI.UnitTests
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _bookingRepoMock;
        private readonly Mock<IRoomRepository> _roomRepoMock;
        private readonly Mock<IIdempotencyService> _idempotencyMock;
        private readonly BookingService _service;

        public BookingServiceTests()
        {
            _bookingRepoMock = new Mock<IBookingRepository>();
            _roomRepoMock = new Mock<IRoomRepository>();
            _idempotencyMock = new Mock<IIdempotencyService>();
            var loggerMock = new Mock<ILogger<BookingService>>();

            _service = new BookingService(
                _bookingRepoMock.Object,
                _roomRepoMock.Object,
                _idempotencyMock.Object,
                loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateBookingAsync_WhenRoomAvailable_ShouldCreateBooking()
        {
            // Arrange
            var room = new Room { Id = 1, RoomNumber = "101", Type = "Single", PricePerNight = 50, IsAvailable = true };
            var request = new BookingRequestDto
            {
                RoomId = 1,
                CustomerName = "Test",
                CustomerEmail = "test@test.com",
                CustomerPhone = "123456789",
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3)
            };

            _roomRepoMock.Setup(r => r.GetAvailableRoomAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(room);

            _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
            _bookingRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateBookingAsync(request, "test-key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.CustomerName, result.CustomerName);
            Assert.Equal(100, result.TotalPrice); // 2 شب * 50
        }

        [Fact]
        public async Task CreateBookingAsync_WhenRoomNotAvailable_ShouldThrowException()
        {
            // Arrange
            _roomRepoMock.Setup(r => r.GetAvailableRoomAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync((Room)null);

            var request = new BookingRequestDto
            {
                RoomId = 1,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3)
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateBookingAsync(request, "test-key")
            );
        }
    }
}