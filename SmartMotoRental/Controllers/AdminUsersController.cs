using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;
using SmartMotoRental.Models;
using SmartMotoRental.ViewModels;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index(string search = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;

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
    }
}
