using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;

namespace SmartMotoRental.Controllers
{
    public class MyRentalsController : Controller
    {
        private readonly SmartMotoRentalContext _context;

        public MyRentalsController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        // GET: MyRentals
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem đơn thuê của bạn";
                return RedirectToAction("Login", "Account");
            }

            var rentals = await _context.Rentals
                .Include(r => r.Motorbike)
                .Where(r => r.UserId == userId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(rentals);
        }

        // GET: MyRentals/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem chi tiết đơn thuê";
                return RedirectToAction("Login", "Account");
            }

            var rental = await _context.Rentals
                .Include(r => r.Motorbike)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RentalId == id && r.UserId == userId.Value);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: MyRentals/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            var rental = await _context.Rentals
                .Include(r => r.Motorbike)
                .FirstOrDefaultAsync(r => r.RentalId == id && r.UserId == userId.Value);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn thuê";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ cho phép hủy đơn ở trạng thái Pending hoặc Confirmed
            if (rental.Status != RentalStatus.Pending && rental.Status != RentalStatus.Confirmed)
            {
                TempData["Error"] = "Không thể hủy đơn thuê này";
                return RedirectToAction(nameof(Details), new { id });
            }

            rental.Status = RentalStatus.Cancelled;
            if (rental.Motorbike != null && rental.Motorbike.Status == MotorbikeStatus.Rented)
            {
                rental.Motorbike.Status = MotorbikeStatus.Available;
            }

            _context.Update(rental);
            if (rental.Motorbike != null)
            {
                _context.Update(rental.Motorbike);
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Hủy đơn thuê thành công! {(!string.IsNullOrEmpty(reason) ? "Lý do: " + reason : "")}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

