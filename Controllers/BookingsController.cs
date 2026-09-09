using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            // دریافت Idempotency Key از هدر (ترجیح داده می‌شود)
            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault() 
                                 ?? request.IdempotencyKey;

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                // می‌توان کلید تولید کرد یا خطا برگرداند
                idempotencyKey = Guid.NewGuid().ToString();
            }

            try
            {
                var result = await _bookingService.CreateBookingAsync(request, idempotencyKey);
                return CreatedAtAction(nameof(GetBookingById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد رزرو");
                return StatusCode(500, "خطای داخلی سرور.");
            }
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                var result = await _bookingService.GetBookingByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/bookings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(id);
                return Ok(new { message = "رزرو با موفقیت لغو شد." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/bookings/room/{roomId}
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetBookingsByRoom(int roomId)
        {
            var result = await _bookingService.GetBookingsByRoomAsync(roomId);
            return Ok(result);
        }
    }
}