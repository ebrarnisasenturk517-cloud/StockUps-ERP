using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class AdminController : Controller
    {
        private readonly StockOpsDbContext _context;

        public AdminController(StockOpsDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.PendingRequests = _context.Requests.Count();
            ViewBag.CriticalStock = _context.Products.Count(x => x.StockQuantity < 20);
            ViewBag.LastRequests = _context.Requests
    .Include(x => x.Product)
    .Include(x => x.User)
    .OrderByDescending(x => x.RequestDate)
    .Take(5)
    .ToList();
            return View();
        }
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.PendingRequests = _context.Requests.Count();
            ViewBag.CriticalStock = _context.Products.Count(x => x.StockQuantity < 20);

            ViewBag.ApprovedRequests = _context.Requests.Count(x => x.Status == "Onaylandı");
            ViewBag.RejectedRequests = _context.Requests.Count(x => x.Status == "Reddedildi");
            ViewBag.WaitingRequests = _context.Requests.Count(x => x.Status == "Bekliyor" || x.Status == "Satın Alma Bekliyor");

            return View();
        }
    }
}