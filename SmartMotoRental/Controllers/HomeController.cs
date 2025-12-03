using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;

namespace SmartMotoRental.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SmartMotoRentalContext _context;

    public HomeController(ILogger<HomeController> logger, SmartMotoRentalContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy danh sách xe có sẵn (tối đa 6 xe)
        var motorbikes = _context.Motorbikes
            .Where(m => m.Status == MotorbikeStatus.Available)
            .Take(6)
            .ToList();
        
        // Lấy thông báo cho user đã đăng nhập
        var userId = HttpContext.Session.GetInt32("UserId");
        List<Notification> notifications = new List<Notification>();
        
        if (userId.HasValue)
        {
            notifications = await _context.Notifications
                .Where(n => n.UserId == userId.Value && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
        else
        {
            // Lấy thông báo hệ thống (Type = System) hoặc thông báo chung
            notifications = await _context.Notifications
                .Where(n => n.Type == NotificationType.System)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
        
        ViewBag.Notifications = notifications;
        
        return View(motorbikes);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string phone, string message)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
        {
            TempData["ContactError"] = "Vui lòng điền đầy đủ thông tin!";
        }
        else
        {
            // TODO: Gửi email hoặc lưu vào database
            TempData["ContactSuccess"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
        }
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

