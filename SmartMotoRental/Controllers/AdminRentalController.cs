using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Models;
using SmartMotoRental.Data;

namespace SmartMotoRental.Controllers
{
    public class AdminRentalController : Controller
    {
        private readonly SmartMotoRentalContext _context;

        public AdminRentalController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        // GET: AdminRental
        public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate, string? userName)
        {
            var rentalsQuery = _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Motorbike)
                .AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RentalStatus>(status, out var rentalStatus))
            {
                rentalsQuery = rentalsQuery.Where(r => r.Status == rentalStatus);
            }

            // Lọc theo ngày bắt đầu (PickupTime)
            if (startDate.HasValue)
            {
                rentalsQuery = rentalsQuery.Where(r => r.PickupTime >= startDate.Value);
            }

            // Lọc theo ngày kết thúc (ReturnTime)
            if (endDate.HasValue)
            {
                rentalsQuery = rentalsQuery.Where(r => r.ReturnTime <= endDate.Value);
            }

            // Lọc theo tên người dùng
            if (!string.IsNullOrEmpty(userName))
            {
                rentalsQuery = rentalsQuery.Where(r => 
                    r.User != null && 
                    (r.User.FullName.Contains(userName) || r.User.Email.Contains(userName))
                );
            }

            var rentals = await rentalsQuery
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Truyền các tham số filter vào ViewBag
            ViewBag.Status = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.UserName = userName;

            return View(rentals);
        }

        // GET: AdminRental/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Motorbike)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: AdminRental/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Motorbike)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn thuê";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trạng thái đơn
            if (rental.Status != RentalStatus.Pending)
            {
                TempData["Error"] = "Đơn thuê này không thể duyệt";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Kiểm tra xe có sẵn không
            if (rental.Motorbike == null || rental.Motorbike.Status != MotorbikeStatus.Available)
            {
                TempData["Error"] = "Xe không sẵn sàng để cho thuê";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Duyệt đơn
            rental.Status = RentalStatus.Confirmed;
            rental.Motorbike.Status = MotorbikeStatus.Rented;
            
            _context.Update(rental);
            _context.Update(rental.Motorbike);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Duyệt đơn thuê thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AdminRental/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var rental = await _context.Rentals.FindAsync(id);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn thuê";
                return RedirectToAction(nameof(Index));
            }

            if (rental.Status != RentalStatus.Pending)
            {
                TempData["Error"] = "Đơn thuê này không thể từ chối";
                return RedirectToAction(nameof(Details), new { id });
            }

            rental.Status = RentalStatus.Cancelled;

            _context.Update(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Từ chối đơn thuê thành công! {(!string.IsNullOrEmpty(reason) ? "Lý do: " + reason : "")}";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AdminRental/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Motorbike)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn thuê";
                return RedirectToAction(nameof(Index));
            }

            if (rental.Status != RentalStatus.Confirmed)
            {
                TempData["Error"] = "Đơn thuê này không thể hoàn thành";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Cập nhật trạng thái đơn
            rental.Status = RentalStatus.Completed;

            // Khi hoàn thành đơn thì đánh dấu đã thanh toán
            // (nếu chưa có thời gian thanh toán thì gán thời gian hiện tại)
            if (!rental.PaymentPaidAt.HasValue)
            {
                rental.PaymentPaidAt = DateTime.UtcNow;
            }

            if (rental.Motorbike != null)
            {
                rental.Motorbike.Status = MotorbikeStatus.Available;
            }

            _context.Update(rental);
            if (rental.Motorbike != null)
            {
                _context.Update(rental.Motorbike);
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hoàn thành đơn thuê thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AdminRental/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var rental = await _context.Rentals
                .Include(r => r.Motorbike)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn thuê";
                return RedirectToAction(nameof(Index));
            }

            if (rental.Status == RentalStatus.Completed || rental.Status == RentalStatus.Cancelled)
            {
                TempData["Error"] = "Đơn thuê này không thể hủy";
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

        // POST: AdminRental/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Motorbike)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn thuê";
                return RedirectToAction(nameof(Index));
            }

            // Nếu xe đang ở trạng thái đang thuê thì trả lại trạng thái sẵn sàng
            if (rental.Motorbike != null && rental.Motorbike.Status == MotorbikeStatus.Rented)
            {
                rental.Motorbike.Status = MotorbikeStatus.Available;
                _context.Update(rental.Motorbike);
            }

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa đơn thuê thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminRental/Statistics
        public async Task<IActionResult> Statistics()
        {
            var stats = new
            {
                TotalRentals = await _context.Rentals.CountAsync(),
                PendingRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Pending),
                ConfirmedRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Confirmed),
                CompletedRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Completed),
                CancelledRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Cancelled),
                TotalRevenue = (await _context.Rentals
                    .Where(r => r.Status == RentalStatus.Completed && r.TotalPrice.HasValue)
                    .Select(r => r.TotalPrice.Value)
                    .ToListAsync()).Sum(),
                AvailableMotorbikes = await _context.Motorbikes.CountAsync(m => m.Status == MotorbikeStatus.Available),
                RentedMotorbikes = await _context.Motorbikes.CountAsync(m => m.Status == MotorbikeStatus.Rented),
                MaintenanceMotorbikes = await _context.Motorbikes.CountAsync(m => m.Status == MotorbikeStatus.Maintenance)
            };

            return View(stats);
        }

        // GET: AdminRental/RecentRentals
        public async Task<IActionResult> RecentRentals(int count = 10)
        {
            var recentRentals = await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Motorbike)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();

            return PartialView("_RecentRentals", recentRentals);
        }
    }
}