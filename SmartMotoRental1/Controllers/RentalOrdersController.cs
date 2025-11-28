using Microsoft.AspNetCore.Mvc;
using SmartMotoRental.Models;
using SmartMotoRental.Services;
using System.Linq;

namespace SmartMotoRental.Controllers
{
    public class RentalOrdersController : Controller
    {
        private readonly IRentalOrderService _rentalOrderService;

        public RentalOrdersController(IRentalOrderService rentalOrderService)
        {
            _rentalOrderService = rentalOrderService;
        }

        // GET: /RentalOrders/Index (Điểm vào chính)
        public async Task<IActionResult> Index(string searchTerm, string statusFilter)
        {
            var orders = await _rentalOrderService.GetFilteredOrdersAsync(searchTerm, statusFilter);
            
            var viewModel = orders.Select(o => new RentalOrderViewModel
            {
                OrderId = o.Id,
                CustomerName = o.Customer?.Name,
                VehicleName = o.Vehicle?.Name,
                // ...
                Status = GetStatusDisplay(o.Status),
                CanBeRented = o.Status == "Pending",
                CanBeCancelled = o.Status == "Pending" 
            }).ToList();

            ViewBag.Statuses = await _rentalOrderService.GetAllStatuses(); 
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentStatus"] = statusFilter;

            return View(viewModel); 
        }

        // POST: /RentalOrders/ChangeStatus (Xử lý các nút Nhận xe/Hủy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            await _rentalOrderService.UpdateOrderStatusAsync(id, newStatus);
            return RedirectToAction("Index");
        }
        
        private string GetStatusDisplay(string status)
        {
            // Chuyển đổi tên trạng thái tiếng Anh sang tiếng Việt cho FE
            return status switch
            {
                "Pending" => "Đang chờ",
                "Rented" => "Đã thuê",
                // ...
                _ => status,
            };
        }
    }
}