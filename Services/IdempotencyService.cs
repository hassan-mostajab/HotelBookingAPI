using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HotelBookingAPI.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(ApplicationDbContext context, ILogger<IdempotencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task StoreResponseAsync<T>(string key, T response) where T : class
        {
            // حذف رکورد قبلی
            var existing = await _context.IdempotencyRecords
                .FirstOrDefaultAsync(r => r.Key == key);
            if (existing != null)
            {
                _context.IdempotencyRecords.Remove(existing);
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            var record = new IdempotencyRecord
            {
                Key = key,
                ResponseBody = jsonResponse,
                StatusCode = 200,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // انقضا پس از ۷ روز
                CreatedAt = DateTime.UtcNow
            };

            await _context.IdempotencyRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Idempotency record برای کلید {Key} ذخیره شد.", key);
        }

        public async Task<T> GetResponseAsync<T>(string key) where T : class
        {
            var record = await _context.IdempotencyRecords
                .FirstOrDefaultAsync(r => r.Key == key && r.ExpiresAt > DateTime.UtcNow);

            if (record == null)
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(record.ResponseBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "خطا در د سریالایز کردن پاسخ برای کلید {Key}", key);
                return null;
            }
        }

        public async Task CleanExpiredRecordsAsync()
        {
            var expired = await _context.IdempotencyRecords
                .Where(r => r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expired.Any())
            {
                _context.IdempotencyRecords.RemoveRange(expired);
                await _context.SaveChangesAsync();
                _logger.LogInformation("{Count} رکورد منقضی حذف شد.", expired.Count);
            }
        }
    }
}