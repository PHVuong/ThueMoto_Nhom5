using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartMotoRental.Data;       
using SmartMotoRental.Models;     // Namespace chứa các Model: User, Xe, DonHang
using SmartMotoRental.ViewModels;

namespace SmartMotoRental.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SmartMotoRentalContext _context; 

        public AdminController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        // ==========================================
        // PHẦN 1: THỐNG KÊ (DASHBOARD) - Read
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(), 
                TotalBikes = await _context.Motorbikes.CountAsync(), 
                TotalOrders = await _context.Rentals.CountAsync(),
                TotalRevenue = await _context.Rentals
                    .Where(r => r.TotalPrice.HasValue)
                    .SumAsync(r => r.TotalPrice.Value)
            };

            var currentYear = DateTime.Now.Year;
            model.ChartLabels = new List<string>();
            model.ChartData = new List<decimal>();

            for (int i = 1; i <= 12; i++)
            {
                model.ChartLabels.Add("Tháng " + i);
                var revenue = await _context.Rentals
                    .Where(r => r.CreatedAt.Year == currentYear && r.CreatedAt.Month == i && r.TotalPrice.HasValue)
                    .SumAsync(r => r.TotalPrice.Value);
                model.ChartData.Add(revenue);
            }

            return View(model);
        }

        // ==========================================
        // PHẦN 2: QUẢN LÝ NGƯỜI DÙNG (CRUD)
        // ==========================================
        
        // 2.1 Xem danh sách (Read)
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Select(u => new UserViewModel
            {
                Id = u.UserId.ToString(),
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                IsActive = true 
            }).ToListAsync();

            return View(users);
        }

        // 2.2 Hiển thị form tạo mới (Create - GET)
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new UserViewModel());
        }

        // 2.3 Xử lý tạo mới (Create - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Lưu ý: Cần xử lý hash password. Tạm thời để mặc định.
                // TODO: Implement password hashing và password input trong form
                var newUser = new User 
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = "TEMP_PASSWORD_HASH", // TODO: Hash password thật
                    Role = UserRole.Customer // Mặc định là Customer
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Users));
            }
            return View(model);
        }


        // 2.4 Hiển thị form sửa (Update - GET)
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null || !int.TryParse(id, out int userId)) 
                return NotFound();

            var user = await _context.Users.FindAsync(userId); 
            if (user == null) return NotFound();

            var model = new UserViewModel
            {
                Id = user.UserId.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = true
            };
            return View(model);
        }

        // 2.5 Xử lý sửa (Update - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (int.TryParse(model.Id, out int userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.FullName = model.FullName;
                        user.Email = model.Email;
                        user.Phone = model.Phone;
                        
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Users));
                    }
                }
            }
            return View(model);
        }

        // 2.6 Xử lý xóa (Delete - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (int.TryParse(id, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Users));
        }
    }
}