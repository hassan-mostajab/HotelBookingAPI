using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // POST: api/payments
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(request);
                return Ok(new
                {
                    payment.Id,
                    payment.BookingId,
                    payment.Amount,
                    payment.Currency,
                    payment.Status,
                    payment.TransactionId,
                    payment.PaymentDate
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش پرداخت");
                return StatusCode(500, "خطای داخلی سرور.");
            }
        }

        // GET: api/payments/booking/{bookingId}
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetPaymentByBookingId(int bookingId)
        {
            var payment = await _paymentService.GetPaymentByBookingIdAsync(bookingId);
            if (payment == null)
                return NotFound($"پرداختی برای رزرو با شناسه {bookingId} یافت نشد.");

            return Ok(payment);
        }
    }
}