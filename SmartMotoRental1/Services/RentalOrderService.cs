using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;

namespace SmartMotoRental.Services
{
    public class RentalOrderService : IRentalOrderService
    {
        private readonly SmartRentalDbContext _context;

        public RentalOrderService(SmartRentalDbContext context)
        {
            _context = context; 
        }

        public async Task<IEnumerable<RentalOrder>> GetFilteredOrdersAsync(string searchTerm, string statusFilter)
        {
            var query = _context.RentalOrders
                .Include(o => o.Customer)
                .Include(o => o.Vehicle)
                // ... (Thêm các Include khác)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Tất cả")
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(o => 
                    o.Id.ToString().Contains(search) || 
                    o.Customer.Name.ToLower().Contains(search) 
                );
            }
            
            return await query.OrderByDescending(o => o.Id).ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string newStatus)
        {
            var order = await _context.RentalOrders.FindAsync(id);
            if (order == null) return false;

            // Logic cập nhật trạng thái
            if (order.Status == "Pending" && (newStatus == "Rented" || newStatus == "Cancelled"))
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public Task<List<string>> GetAllStatuses()
        {
            return Task.FromResult(new List<string> { "Pending", "Rented", "Returned", "Cancelled" });
        }
    }
}