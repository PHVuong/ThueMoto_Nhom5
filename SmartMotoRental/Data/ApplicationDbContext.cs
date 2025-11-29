using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Models;
using System;

namespace SmartMotoRental.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
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
            // Note: Seed data đã được xử lý trong SmartMotoRentalContext và SeedData.cs
        }
    }
}