using Microsoft.AspNetCore.Mvc;
using SmartMotoRental.Data;
using SmartMotoRental.Models;

namespace SmartMotoRental.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly SmartMotoRentalContext _context;

        public NotificationsController(SmartMotoRentalContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var list = _context.Notifications
                               .OrderByDescending(n => n.CreatedAt)
                               .ToList();
            return View(list);
        }

        public IActionResult Delete(int id)
        {
            var noti = _context.Notifications.Find(id);
            if (noti != null)
            {
                _context.Notifications.Remove(noti);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteAll()
        {
            _context.Notifications.RemoveRange(_context.Notifications);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
