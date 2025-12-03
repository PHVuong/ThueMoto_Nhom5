using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;
using SmartMotoRental.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SmartMotoRental.Controllers
{
    public class AdminUsersController : Controller
    {
        private readonly SmartMotoRentalContext _context;

        public AdminUsersController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        // GET: /AdminUsers
        public async Task<IActionResult> Index(string search = "", string status = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Users
                .Include(u => u.Rentals)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            // Tính toán thống kê tổng quan
            var totalUsers = await _context.Users.CountAsync();
            var lastMonthUsers = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                .CountAsync();
            var userGrowthPercent = lastMonthUsers > 0 ? (int)Math.Round((double)(totalUsers - lastMonthUsers) / lastMonthUsers * 100) : 0;

            var totalRentals = await _context.Rentals.CountAsync();
            var lastMonthRentals = await _context.Rentals
                .Where(r => r.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                .CountAsync();
            var rentalGrowthPercent = lastMonthRentals > 0 ? (int)Math.Round((double)(totalRentals - lastMonthRentals) / lastMonthRentals * 100) : 0;

            // SQLite không hỗ trợ Sum trên decimal, cần load về client trước
            var totalRevenue = (await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed && r.TotalPrice.HasValue)
                .Select(r => r.TotalPrice ?? 0m)
                .ToListAsync()).Sum();
            var lastMonthRevenue = (await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed && r.TotalPrice.HasValue && r.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                .Select(r => r.TotalPrice ?? 0m)
                .ToListAsync()).Sum();
            var revenueGrowthPercent = lastMonthRevenue > 0 ? (int)Math.Round((double)(totalRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100) : 0;

            var avgRevenuePerUser = totalUsers > 0 ? totalRevenue / totalUsers : 0m;
            var lastMonthAvgRevenuePerUser = lastMonthUsers > 0 ? lastMonthRevenue / lastMonthUsers : 0m;
            var avgRevenueGrowthPercent = lastMonthAvgRevenuePerUser > 0 ? (int)Math.Round((double)(avgRevenuePerUser - lastMonthAvgRevenuePerUser) / (double)lastMonthAvgRevenuePerUser * 100) : 0;

            var total = await query.CountAsync();
            var users = await query
                .Include(u => u.Rentals)
                .Include(u => u.Reviews)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.UserGrowthPercent = userGrowthPercent;
            ViewBag.TotalRentals = totalRentals;
            ViewBag.RentalGrowthPercent = rentalGrowthPercent;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RevenueGrowthPercent = revenueGrowthPercent;
            ViewBag.AvgRevenuePerUser = avgRevenuePerUser;
            ViewBag.AvgRevenueGrowthPercent = avgRevenueGrowthPercent;

            return View(users);
        }

        // GET: /AdminUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                PasswordHash = "TEMP_PASSWORD_HASH", // TODO: Implement password hashing
                Role = UserRole.Customer // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminUsers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new UserViewModel
            {
                Id = user.UserId.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone
            };
            return View(vm);
        }

        // POST: /AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel vm)
        {
            if (id.ToString() != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.Phone = vm.Phone;

            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminUsers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // API: Thống kê thuê xe máy theo ngày (7 ngày qua)
        [HttpGet]
        public async Task<IActionResult> GetRentalStats(int days = 7)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var rentals = await _context.Rentals
                .Where(r => r.CreatedAt >= fromDate)
                .ToListAsync();

            var stats = Enumerable.Range(0, days)
                .Select(i =>
                {
                    var date = DateTime.UtcNow.AddDays(-i);
                    var dayLabel = date.DayOfWeek switch
                    {
                        DayOfWeek.Monday => "T2",
                        DayOfWeek.Tuesday => "T3",
                        DayOfWeek.Wednesday => "T4",
                        DayOfWeek.Thursday => "T5",
                        DayOfWeek.Friday => "T6",
                        DayOfWeek.Saturday => "T7",
                        DayOfWeek.Sunday => "CN",
                        _ => ""
                    };
                    var count = rentals.Count(r => r.CreatedAt.Date == date.Date);
                    return new { label = dayLabel, count };
                })
                .Reverse()
                .ToList();

            return Json(stats);
        }

        // API: Doanh thu theo tháng
        [HttpGet]
        public async Task<IActionResult> GetRevenueByMonth(int year = 0)
        {
            if (year == 0) year = DateTime.UtcNow.Year;

            var rentals = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed && r.TotalPrice.HasValue && r.CreatedAt.Year == year)
                .ToListAsync();

            var stats = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var monthLabel = $"T{month}";
                    var revenue = rentals
                        .Where(r => r.CreatedAt.Month == month)
                        .Sum(r => r.TotalPrice ?? 0);
                    return new { label = monthLabel, revenue = (int)(revenue / 1000000) }; // Chuyển sang triệu VNĐ
                })
                .ToList();

            return Json(stats);
        }

        // API: Xe máy phổ biến
        [HttpGet]
        public async Task<IActionResult> GetPopularBikes(string period = "month")
        {
            DateTime fromDate = period switch
            {
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddMonths(-1),
                "year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddMonths(-1)
            };

            var popularBikes = await _context.Rentals
                .Include(r => r.Motorbike)
                .Where(r => r.CreatedAt >= fromDate && r.Motorbike != null)
                .GroupBy(r => new { r.Motorbike.BikeId, r.Motorbike.Name })
                .Select(g => new
                {
                    name = g.Key.Name,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToListAsync();

            var totalRentals = popularBikes.Sum(x => x.count);
            var popularBikeNames = popularBikes.Select(p => p.name).ToList();
            
            var result = popularBikes.Select(b => new
            {
                name = b.name,
                count = b.count,
                percentage = totalRentals > 0 ? (int)Math.Round((double)b.count / totalRentals * 100) : 0
            }).ToList();

            // Thêm "Khác" nếu có
            if (popularBikes.Count == 5)
            {
                var otherCount = await _context.Rentals
                    .Include(r => r.Motorbike)
                    .Where(r => r.CreatedAt >= fromDate && r.Motorbike != null && 
                        !popularBikeNames.Contains(r.Motorbike.Name))
                    .CountAsync();
                if (otherCount > 0)
                {
                    result.Add(new
                    {
                        name = "Khác",
                        count = otherCount,
                        percentage = totalRentals > 0 ? (int)Math.Round((double)otherCount / totalRentals * 100) : 0
                    });
                }
            }

            return Json(result);
        }

        // Xuất CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            var users = await _context.Users
                .Include(u => u.Rentals)
                .ToListAsync();

            var csv = "ID,Họ tên,Email,Số điện thoại,Số lần thuê,Tổng chi tiêu,Trạng thái\n";
            foreach (var user in users)
            {
                var rentalCount = user.Rentals.Count(r => r.Status == RentalStatus.Completed);
                var totalSpending = user.Rentals
                    .Where(r => r.Status == RentalStatus.Completed && r.TotalPrice.HasValue)
                    .Sum(r => r.TotalPrice ?? 0);
                csv += $"{user.UserId},{user.FullName},{user.Email},{user.Phone ?? ""},{rentalCount},{totalSpending},Hoạt động\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"users_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Xuất Excel (CSV format với extension .xlsx)
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            // Tạm thời xuất CSV với extension .xlsx
            // Để xuất Excel thật cần thêm thư viện như EPPlus hoặc ClosedXML
            return await ExportCsv();
        }
    }
}
