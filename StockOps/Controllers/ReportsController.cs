using Microsoft.AspNetCore.Mvc;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class ReportsController : Controller
    {
        private readonly StockOpsDbContext _context;

        public ReportsController(StockOpsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            var products = _context.Products.ToList();

            ViewBag.TotalProducts = products.Count;
            ViewBag.CriticalStock = products.Count(x => x.StockQuantity < 20);
            ViewBag.OutOfStock = products.Count(x => x.StockQuantity == 0);
            ViewBag.EnoughStock = products.Count(x => x.StockQuantity >= 20);

            var categoryReports = products
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalStock = g.Sum(x => x.StockQuantity)
                })
                .OrderByDescending(x => x.TotalStock)
                .ToList();

            ViewBag.CategoryReports = categoryReports;

            var criticalProducts = products
                .Where(x => x.StockQuantity < 20)
                .OrderBy(x => x.StockQuantity)
                .ToList();

            return View(criticalProducts);
        }
    }
}
