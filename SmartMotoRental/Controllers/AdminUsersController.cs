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
                TotalBikes = await _context.Xes.CountAsync(), 
                TotalOrders = await _context.DonHangs.CountAsync(),
                TotalRevenue = await _context.DonHangs.SumAsync(x => x.TongTien)
            };

            var currentYear = DateTime.Now.Year;
            model.ChartLabels = new List<string>();
            model.ChartData = new List<decimal>();

            for (int i = 1; i <= 12; i++)
            {
                model.ChartLabels.Add("Tháng " + i);
                var revenue = await _context.DonHangs
                    .Where(d => d.NgayDat.Year == currentYear && d.NgayDat.Month == i)
                    .SumAsync(d => d.TongTien);
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
                Id = u.Id.ToString(),
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
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
                // Lưu ý: Cần xử lý hash password nếu dùng ASP.NET Identity. 
                // Đây là ví dụ đơn giản với EF Core.
                var newUser = new User 
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    // Thêm các trường khác cần thiết (ví dụ: set Role mặc định)
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
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(Guid.Parse(id)); 
            if (user == null) return NotFound();

            var model = new UserViewModel
            {
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
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
                var user = await _context.Users.FindAsync(Guid.Parse(model.Id));
                if (user != null)
                {
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Users));
                }
            }
            return View(model);
        }

        // 2.6 Xử lý xóa (Delete - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(Guid.Parse(id));
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }
    }
}