using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;

namespace SmartMotoRental.Controllers
{
    public class DatThueXeController : Controller
    {
        private readonly SmartMotoRentalContext _context;

        public DatThueXeController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        // GET: /DatThueXe?bikeId=1
        public async Task<IActionResult> Index(int? bikeId)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt thuê xe";
                // Lưu returnUrl để sau khi đăng nhập quay lại trang này
                var returnUrl = bikeId.HasValue 
                    ? $"/DatThueXe?bikeId={bikeId.Value}" 
                    : "/DatThueXe";
                return RedirectToAction("Login", "Account", new { returnUrl });
            }
            
            // Lấy danh sách xe có sẵn để hiển thị trong dropdown
            var availableBikes = await _context.Motorbikes
                .Where(m => m.Status == MotorbikeStatus.Available)
                .OrderBy(m => m.Name)
                .ToListAsync();
            
            ViewBag.AvailableBikes = availableBikes;
            
            if (bikeId.HasValue)
            {
                var bike = await _context.Motorbikes.FindAsync(bikeId.Value);
                if (bike == null)
                {
                    TempData["Error"] = "Không tìm thấy xe máy";
                    return RedirectToAction("Index", "Motorbike");
                }

                if (bike.Status != MotorbikeStatus.Available)
                {
                    TempData["Error"] = "Xe máy này hiện không có sẵn";
                    return RedirectToAction("Details", "Motorbike", new { id = bikeId });
                }

                ViewBag.Bike = bike;
                ViewBag.BikeId = bikeId.Value;
            }

            return View();
        }

        // POST: /DatThueXe/DatXe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatXe(RentalOrder order)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt thuê xe";
                return RedirectToAction("Login", "Account");
            }
            
            if (order.BikeId <= 0)
            {
                ModelState.AddModelError("BikeId", "Vui lòng chọn xe máy");
            }

            if (order.PickupDate >= order.ReturnDate)
            {
                ModelState.AddModelError("ReturnDate", "Ngày trả xe phải sau ngày nhận xe");
            }

            if (order.PickupDate < DateTime.Now)
            {
                ModelState.AddModelError("PickupDate", "Ngày nhận xe không thể trong quá khứ");
            }

            if (!ModelState.IsValid)
            {
                // Lấy danh sách xe có sẵn để hiển thị trong dropdown
                var availableBikes = await _context.Motorbikes
                    .Where(m => m.Status == MotorbikeStatus.Available)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
                
                ViewBag.AvailableBikes = availableBikes;
                
                if (order.BikeId > 0)
                {
                    var bike = await _context.Motorbikes.FindAsync(order.BikeId);
                    ViewBag.Bike = bike;
                    ViewBag.BikeId = order.BikeId;
                }
                return View("Index", order);
            }

            // Kiểm tra xe có sẵn không
            var motorbike = await _context.Motorbikes.FindAsync(order.BikeId);
            if (motorbike == null)
            {
                TempData["Error"] = "Không tìm thấy xe máy";
                return RedirectToAction("Index", "Motorbike");
            }

            if (motorbike.Status != MotorbikeStatus.Available)
            {
                TempData["Error"] = "Xe máy này hiện không có sẵn";
                return RedirectToAction("Details", "Motorbike", new { id = order.BikeId });
            }

            // Tính giá thuê
            var timeSpan = order.ReturnDate - order.PickupDate;
            var totalHours = (int)Math.Ceiling(timeSpan.TotalHours);
            var totalDays = (int)Math.Ceiling(timeSpan.TotalDays);

            decimal? estimatedPrice = null;
            if (totalDays >= 1 && motorbike.PricePerDay.HasValue)
            {
                estimatedPrice = motorbike.PricePerDay.Value * totalDays;
            }
            else if (motorbike.PricePerHour.HasValue)
            {
                estimatedPrice = motorbike.PricePerHour.Value * totalHours;
            }

            order.EstimatedPrice = estimatedPrice;
            order.CreatedAt = DateTime.Now;

            // Lưu đơn đặt thuê
            _context.RentalOrders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo đơn thuê (Rental) với trạng thái Pending
            var rental = new Rental
            {
                BikeId = order.BikeId,
                UserId = userId.Value, // userId đã được kiểm tra != null ở trên
                PickupTime = order.PickupDate,
                ReturnTime = order.ReturnDate,
                TotalPrice = estimatedPrice,
                Status = RentalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt xe thành công! Vui lòng chờ xác nhận từ chúng tôi.";
            return RedirectToAction("Index", "Home");
        }
    }
}
