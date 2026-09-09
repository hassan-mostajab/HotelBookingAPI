using System.Net;
using System.Text.Json;

namespace HotelBookingAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = exception.GetType().Name,
                message = exception.Message,
                // در محیط تولید (Production) جزئیات استک را نمایش ندهید
                stackTrace = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                    ? exception.StackTrace
                    : null
            };

            context.Response.StatusCode = exception switch
            {
                KeyNotFoundException _ => (int)HttpStatusCode.NotFound,
                ArgumentException _ => (int)HttpStatusCode.BadRequest,
                InvalidOperationException _ => (int)HttpStatusCode.Conflict,
                DbUpdateConcurrencyException _ => (int)HttpStatusCode.Conflict,
                UnauthorizedAccessException _ => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}