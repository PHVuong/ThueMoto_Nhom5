using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Models;
using System;

namespace SmartMotoRental.Data
{
    public class SmartMotoRentalContext : DbContext
    {
        public SmartMotoRentalContext(DbContextOptions<SmartMotoRentalContext> options)
            : base(options)
        {
        }

        // Các DbSet sẵn có (giữ nguyên)
        public DbSet<User> Users { get; set; }  // <-- mới
        public DbSet<Motorbike> Motorbikes { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nếu đã có seed data khác, merge tránh duplicate key issues.
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "admin", Email = "admin@example.com", Role = "Admin", CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new User { Id = 2, UserName = "nguyenvan", Email = "nguyenvan@example.com", Role = "User", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
                new User { Id = 3, UserName = "tranthi", Email = "tranthi@example.com", Role = "User", CreatedAt = DateTime.UtcNow.AddDays(-10) }
            );
        }
    }
}