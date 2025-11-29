using SmartMotoRental.Models;

namespace SmartMotoRental.Services
{
    public interface IRentalOrderService
    {
        Task<IEnumerable<RentalOrder>> GetFilteredOrdersAsync(string searchTerm, string statusFilter);
        Task<bool> UpdateOrderStatusAsync(int id, string newStatus);
        Task<List<string>> GetAllStatuses();
    }
}