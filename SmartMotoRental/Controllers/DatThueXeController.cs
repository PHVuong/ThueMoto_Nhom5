using Microsoft.AspNetCore.Mvc;
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

        // GET: /DatThueXe
        public IActionResult Index()
        {
            return View();
        }

        // POST: /DatThueXe/DatXe
        [HttpPost]
        public async Task<IActionResult> DatXe(RentalOrder order)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", order);
            }

            _context.RentalOrders.Add(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt xe thành công!";
            return RedirectToAction("Index");
        }
    }
}
