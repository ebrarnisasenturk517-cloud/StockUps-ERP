using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class PurchasingController : Controller
    {
        private readonly StockOpsDbContext _context;

        public PurchasingController(StockOpsDbContext context)
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

            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalRequests = _context.Requests.Count();

            ViewBag.WaitingApproval = _context.Requests
                .Count(x => x.Status == "Bekliyor");

            ViewBag.WaitingStockOut = _context.Requests
                .Count(x => x.Status == "Satın Alma Onayladı" || x.Status == "Onaylandı");

            ViewBag.CompletedRequests = _context.Requests
                .Count(x => x.Status == "Stok Çıkışı Yapıldı");

            ViewBag.RejectedRequests = _context.Requests
                .Count(x => x.Status == "Reddedildi");

            ViewBag.CriticalStock = _context.Products
                .Count(x => x.StockQuantity < 20);

            ViewBag.StockOutNotifications = _context.Requests
                .Count(x => x.Status == "Stok Çıkışı Yapıldı");

            return View();
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalRequests = _context.Requests.Count();

            ViewBag.WaitingApproval = _context.Requests
                .Count(x => x.Status == "Bekliyor");

            ViewBag.WaitingStockOut = _context.Requests
                .Count(x => x.Status == "Satın Alma Onayladı" || x.Status == "Onaylandı");

            ViewBag.CompletedRequests = _context.Requests
                .Count(x => x.Status == "Stok Çıkışı Yapıldı");

            ViewBag.RejectedRequests = _context.Requests
                .Count(x => x.Status == "Reddedildi");

            ViewBag.CriticalStock = _context.Products
                .Count(x => x.StockQuantity < 20);

            ViewBag.StockOutNotifications = _context.Requests
                .Count(x => x.Status == "Stok Çıkışı Yapıldı");

            return View();
        }

        public IActionResult DepoTalepleri()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            var requests = _context.Requests
                .Include(x => x.Product)
                .Include(x => x.User)
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            return View(requests);
        }
    }
}