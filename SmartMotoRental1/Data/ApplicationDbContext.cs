using Microsoft.EntityFrameworkCore;
using YourProjectName.Models; // Thay bằng namespace của bạn

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RentalOrder> RentalOrders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Branch> Branches { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Thiết lập mối quan hệ giữa các Entity
        modelBuilder.Entity<RentalOrder>()
            .HasOne(o => o.BranchStart)
            .WithMany()
            .HasForeignKey(o => o.BranchStartId)
            .OnDelete(DeleteBehavior.Restrict); // Tránh xóa cascade

        modelBuilder.Entity<RentalOrder>()
            .HasOne(o => o.BranchEnd)
            .WithMany()
            .HasForeignKey(o => o.BranchEndId)
            .OnDelete(DeleteBehavior.Restrict); 

        // Đảm bảo dữ liệu mẫu (Seeding) nếu cần thiết
        base.OnModelCreating(modelBuilder);
    }
}