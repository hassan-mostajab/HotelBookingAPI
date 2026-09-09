using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تنظیمات اضافی برای اطمینان از RowVersion در MySQL
            modelBuilder.Entity<Room>()
                .Property(r => r.RowVersion)
                .IsRowVersion();

            // ایندکس برای بهبود کارایی جستجو
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate });

            // کلید یکتا برای IdempotencyKey در Booking
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.IdempotencyKey)
                .IsUnique();

            // ایندکس برای IdempotencyRecord
            modelBuilder.Entity<IdempotencyRecord>()
                .HasIndex(i => i.Key)
                .IsUnique();
        }
    }
}