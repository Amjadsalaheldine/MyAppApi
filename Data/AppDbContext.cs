using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Models;

namespace MyAppApi.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

 
        public DbSet<Location> Locations { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Model> Models { get; set; }

    
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<Repair> Repairs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // العلاقة بين Car و Model
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Model)
                .WithMany(m => m.Cars)
                .HasForeignKey(c => c.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين Car و Brand
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Brand)
                .WithMany(b => b.Cars)
                .HasForeignKey(c => c.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين Model و Brand
            modelBuilder.Entity<Model>()
                .HasOne(m => m.Brand)
                .WithMany(b => b.Models)
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين Workshop و Repair
            modelBuilder.Entity<Workshop>()
                .HasMany(w => w.Repairs)
                .WithOne(r => r.Workshop)
                .HasForeignKey(r => r.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين Repair و Car
            modelBuilder.Entity<Repair>()
                .HasOne(r => r.Car)
                .WithMany(c => c.Repairs)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين Payment و Repair
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Repair)
                .WithMany(r => r.Payments) // إذا كنت تريد أن يكون للإصلاح أكثر من دفعة
                .HasForeignKey(p => p.RepairId)
                .OnDelete(DeleteBehavior.Restrict); // أو Cascade إذا أردت حذف الدفعات عند حذف الإصلاح

            // العلاقة بين Payment و Booking
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments) // إذا كنت تريد أن يكون للحجز أكثر من دفعة
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict); // أو Cascade إذا أردت حذف الدفعات عند حذف الحجز
        }
    }
}