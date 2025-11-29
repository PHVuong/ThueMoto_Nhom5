using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMotoRental.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly SmartMotoRentalContext _context;
        public StatisticsController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API: users per role
        [HttpGet]
        public async Task<IActionResult> UsersPerRole()
        {
            var data = await _context.Users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();
            return Json(data);
        }

        // API: users per month (last N months)
        [HttpGet]
        public async Task<IActionResult> UsersPerMonth(int months = 6)
        {
            var from = DateTime.UtcNow.AddMonths(-months + 1);
            var data = await _context.Users
                .Where(u => u.CreatedAt >= from)
                .ToListAsync();

            var result = Enumerable.Range(0, months)
                .Select(i => {
                    var dt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
                    var label = dt.ToString("yyyy-MM");
                    var count = data.Count(u => u.CreatedAt.Year == dt.Year && u.CreatedAt.Month == dt.Month);
                    return new { label, count };
                })
                .OrderBy(x => x.label)
                .ToList();

            return Json(result);
        }
    }
}