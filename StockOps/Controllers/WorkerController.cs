using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class WorkerController : Controller
    {
        private readonly StockOpsDbContext _context;

        public WorkerController(StockOpsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Depo" && role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalProducts =
                _context.Products.Count();

            ViewBag.TotalRequests =
                _context.Requests.Count();

            ViewBag.CriticalStock =
                _context.Products.Count(x =>
                    x.StockQuantity < 20);

            ViewBag.PendingStockOut =
                _context.Requests.Count(x =>
                    x.Status == "Satın Alma Onayladı");

            ViewBag.AvailableLocations =
                _context.StorageLocations.Count(x =>
                    x.IsActive && !x.IsOccupied);

            ViewBag.OccupiedLocations =
                _context.StorageLocations.Count(x =>
                    x.IsActive && x.IsOccupied);

            ViewBag.InactiveLocations =
                _context.StorageLocations.Count(x =>
                    !x.IsActive);

            ViewBag.TodayEntries =
                _context.StockTransactions.Count(x =>
                    x.TransactionDate.Date == DateTime.Today &&
                    x.TransactionType.Contains("Giriş"));

            ViewBag.TodayExits =
                _context.StockTransactions.Count(x =>
                    x.TransactionDate.Date == DateTime.Today &&
                    x.TransactionType.Contains("Çıkış"));

            ViewBag.LastTransactions =
                _context.StockTransactions
                    .Include(x => x.Product)
                    .Include(x => x.Warehouse)
                    .OrderByDescending(x => x.TransactionDate)
                    .Take(6)
                    .ToList();

            return View("~/Views/Worker/Index.cshtml");
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}