using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Models;

namespace SmartMotoRental.Data
{
    public class SmartRentalDbContext : DbContext
    {
        public SmartRentalDbContext(DbContextOptions<SmartRentalDbContext> options)
            : base(options)
        {
        }

        public DbSet<RentalOrder> RentalOrders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Branch> Branches { get; set; }
        
    }
}