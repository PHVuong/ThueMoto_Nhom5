using Microsoft.AspNetCore.Mvc;
using SmartMotoRental.Models;
using SmartMotoRental.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace SmartMotoRental.Controllers
{
    public class AdminMotorbikeController : Controller
    {
        private readonly SmartMotoRentalContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminMotorbikeController(SmartMotoRentalContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: AdminMotorbike
        public async Task<IActionResult> Index()
        {
            var motorbikes = await _context.Motorbikes
                .OrderByDescending(m => m.BikeId)
                .ToListAsync();
            return View(motorbikes);
        }

        // GET: AdminMotorbike/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminMotorbike/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Motorbike motorbike, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra biển số đã tồn tại chưa
                var existingBike = await _context.Motorbikes
                    .FirstOrDefaultAsync(m => m.PlateNumber == motorbike.PlateNumber);
                
                if (existingBike != null)
                {
                    ModelState.AddModelError("PlateNumber", "Biển số xe này đã tồn tại trong hệ thống");
                    return View(motorbike);
                }

                // Xử lý upload ảnh
                if (image != null && image.Length > 0)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    string uploadPath = Path.Combine(wwwRootPath, "images", "motorbikes");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string imagePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    motorbike.ImageUrl = "/images/motorbikes/" + fileName;
                }

                motorbike.Status = MotorbikeStatus.Available;
                motorbike.Condition = MotorbikeCondition.Good;
                
                _context.Motorbikes.Add(motorbike);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(motorbike);
        }

        // GET: AdminMotorbike/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var motorbike = await _context.Motorbikes.FindAsync(id);
            if (motorbike == null)
            {
                return NotFound();
            }
            return View(motorbike);
        }

        // POST: AdminMotorbike/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Motorbike motorbike, IFormFile? image)
        {
            if (id != motorbike.BikeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBike = await _context.Motorbikes.FindAsync(id);
                    if (existingBike == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra biển số trùng (trừ xe hiện tại)
                    var duplicateBike = await _context.Motorbikes
                        .FirstOrDefaultAsync(m => m.PlateNumber == motorbike.PlateNumber && m.BikeId != id);
                    
                    if (duplicateBike != null)
                    {
                        ModelState.AddModelError("PlateNumber", "Biển số xe này đã tồn tại trong hệ thống");
                        return View(motorbike);
                    }

                    // Cập nhật thông tin
                    existingBike.Name = motorbike.Name;
                    existingBike.Type = motorbike.Type;
                    existingBike.Brand = motorbike.Brand;
                    existingBike.Year = motorbike.Year;
                    existingBike.PlateNumber = motorbike.PlateNumber;
                    existingBike.Condition = motorbike.Condition;
                    existingBike.Status = motorbike.Status;
                    existingBike.PricePerHour = motorbike.PricePerHour;
                    existingBike.PricePerDay = motorbike.PricePerDay;
                    existingBike.Location = motorbike.Location;
                    existingBike.Description = motorbike.Description;

                    // Xử lý upload ảnh mới
                    if (image != null && image.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingBike.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingBike.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Upload ảnh mới
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string uploadPath = Path.Combine(wwwRootPath, "images", "motorbikes");
                        
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string imagePath = Path.Combine(uploadPath, fileName);

                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        existingBike.ImageUrl = "/images/motorbikes/" + fileName;
                    }

                    _context.Update(existingBike);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật xe thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MotorbikeExists(motorbike.BikeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(motorbike);
        }

        // GET: AdminMotorbike/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var motorbike = await _context.Motorbikes
                .Include(m => m.Rentals)
                .FirstOrDefaultAsync(m => m.BikeId == id);
            
            if (motorbike == null)
            {
                return NotFound();
            }
            return View(motorbike);
        }

        // POST: AdminMotorbike/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var motorbike = await _context.Motorbikes
                .Include(m => m.Rentals)
                .FirstOrDefaultAsync(m => m.BikeId == id);
            
            if (motorbike != null)
            {
                // Kiểm tra xem xe có đơn thuê đang hoạt động không
                var activeRentals = motorbike.Rentals
                    .Where(r => r.Status == RentalStatus.Pending || r.Status == RentalStatus.Confirmed)
                    .Any();

                if (activeRentals)
                {
                    TempData["Error"] = "Không thể xóa xe đang có đơn thuê hoạt động!";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa ảnh
                if (!string.IsNullOrEmpty(motorbike.ImageUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, motorbike.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Motorbikes.Remove(motorbike);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Xóa xe thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: AdminMotorbike/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var motorbike = await _context.Motorbikes.FindAsync(id);
            if (motorbike == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            // Parse string status to enum
            if (Enum.TryParse<MotorbikeStatus>(status, out var motorbikeStatus))
            {
                motorbike.Status = motorbikeStatus;
                _context.Update(motorbike);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }

            return Json(new { success = false, message = "Trạng thái không hợp lệ" });
        }

        // POST: AdminMotorbike/UpdateCondition
        [HttpPost]
        public async Task<IActionResult> UpdateCondition(int id, string condition)
        {
            var motorbike = await _context.Motorbikes.FindAsync(id);
            if (motorbike == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            if (Enum.TryParse<MotorbikeCondition>(condition, out var motorbikeCondition))
            {
                motorbike.Condition = motorbikeCondition;
                _context.Update(motorbike);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật tình trạng thành công" });
            }

            return Json(new { success = false, message = "Tình trạng không hợp lệ" });
        }

        private bool MotorbikeExists(int id)
        {
            return _context.Motorbikes.Any(e => e.BikeId == id);
        }
    }
}