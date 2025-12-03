using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;
using System.Security.Cryptography;
using System.Text;

namespace SmartMotoRental.Controllers
{
    public class AccountController : Controller
    {
        private readonly SmartMotoRentalContext _context;

        public AccountController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login(string? returnUrl)
        {
            if (IsLoggedIn())
            {
                // Nếu có returnUrl, redirect về đó, nếu không thì về Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Kiểm tra mật khẩu (đơn giản hóa - trong production nên dùng bcrypt)
            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Lưu thông tin user vào session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetInt32("UserRole", (int)user.Role);

            if (rememberMe)
            {
                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = false // Set true nếu dùng HTTPS
                };
                Response.Cookies.Append("RememberMe", user.UserId.ToString(), options);
            }

            TempData["Success"] = $"Chào mừng {user.FullName}!";
            
            // Nếu có returnUrl và là URL hợp lệ, redirect về đó
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Redirect dựa trên role
            if (user.Role == UserRole.Admin || user.Role == UserRole.Staff)
            {
                return RedirectToAction("Index", "Admin");
            }
            
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string phone, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp";
                return View();
            }

            if (password.Length < 6)
            {
                TempData["Error"] = "Mật khẩu phải có ít nhất 6 ký tự";
                return View();
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                TempData["Error"] = "Email này đã được sử dụng";
                return View();
            }

            // Tạo user mới
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                PasswordHash = HashPassword(password),
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RememberMe");
            TempData["Success"] = "Đăng xuất thành công";
            return RedirectToAction("Index", "Home");
        }

        // Helper methods
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        private string HashPassword(string password)
        {
            // Đơn giản hóa - trong production nên dùng BCrypt hoặc ASP.NET Identity
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}

