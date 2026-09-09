using HotelBookingAPI.Data;
using HotelBookingAPI.Middleware;
using HotelBookingAPI.Repositories;
using HotelBookingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. افزودن DbContext با MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. ثبت ریپازیتوری‌ها
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// 3. ثبت سرویس‌ها
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// 4. افزودن کنترلرها
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 5. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Booking API",
        Version = "v1",
        Description = "API برای مدیریت رزرو هتل با پشتیبانی از Optimistic Concurrency و Idempotency"
    });
});

// 6. CORS (اختیاری)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 7. میدلورها
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

// میدلور سفارشی مدیریت خطا
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// 8. اجرای Migration به‌صورت خودکار (اختیاری)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    
    // Seed داده نمونه (اختیاری)
    if (!dbContext.Rooms.Any())
    {
        dbContext.Rooms.AddRange(
            new Room { RoomNumber = "101", Type = "Single", PricePerNight = 50, Capacity = 1, IsAvailable = true },
            new Room { RoomNumber = "102", Type = "Double", PricePerNight = 80, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "201", Type = "Suite", PricePerNight = 150, Capacity = 4, IsAvailable = true }
        );
        await dbContext.SaveChangesAsync();
    }
}

app.Run();