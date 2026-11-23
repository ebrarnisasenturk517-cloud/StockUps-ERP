using Microsoft.AspNetCore.Mvc;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class DashboardController : Controller
    {
        private readonly StockOpsDbContext _context;

        public DashboardController(StockOpsDbContext context)
        {
            _context = context;
        }

        // DEPO ANA PANELİ
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalRequests = _context.Requests.Count();
            ViewBag.CriticalStock = _context.Products.Count(x => x.StockQuantity < 20);

            return View();
        }
    }
}