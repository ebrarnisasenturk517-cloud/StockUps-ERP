using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;
using StockOps.Models;

namespace StockOps.Controllers
{
    public class RequestsController : Controller
    {
        private readonly StockOpsDbContext _context;

        public RequestsController(StockOpsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo" && role != "SatinAlma")
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

        public IActionResult StockOutWaiting()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo")
            {
                return RedirectToAction("Login", "Auth");
            }

            var requests = _context.Requests
                .Include(x => x.Product)
                .Include(x => x.User)
                .Where(x => x.Status == "Satın Alma Onayladı" || x.Status == "Onaylandı")
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            return View(requests);
        }

        [HttpGet]
        public IActionResult Create(int productId)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo")
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = _context.Products.FirstOrDefault(x => x.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;

            return View();
        }

        [HttpPost]
        public IActionResult Create(int productId, int quantity)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Talep miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction("Create", new { productId = productId });
            }

            var product = _context.Products.FirstOrDefault(x => x.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("Index", "Products");
            }

            var username = HttpContext.Session.GetString("Username");

            var user = _context.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                user = _context.Users.FirstOrDefault(x => x.Username == "depo");
            }

            if (user == null)
            {
                TempData["ErrorMessage"] = "Depo kullanıcısı SQL'de bulunamadı.";
                return RedirectToAction("Index", "Worker");
            }

            var request = new Request
            {
                ProductId = product.Id,
                UserId = user.Id,
                Quantity = quantity,
                RequestDate = DateTime.Now,
                Status = "Bekliyor",
                IsNotified = false
            };

            _context.Requests.Add(request);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Talep başarıyla oluşturuldu.";

            return RedirectToAction("Index", "Requests");
        }

        public IActionResult Approve(int id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            var request = _context.Requests.FirstOrDefault(x => x.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != "Bekliyor")
            {
                TempData["ErrorMessage"] = "Bu talep daha önce işleme alınmış.";
                return RedirectToAction("DepoTalepleri", "Purchasing");
            }

            request.Status = "Satın Alma Onayladı";
            request.IsNotified = false;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Talep satın alma tarafından onaylandı. Stok çıkışı depo sorumlusu tarafından yapılacak.";

            return RedirectToAction("DepoTalepleri", "Purchasing");
        }

        public IActionResult Reject(int id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            var request = _context.Requests.FirstOrDefault(x => x.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != "Bekliyor")
            {
                TempData["ErrorMessage"] = "Bu talep daha önce işleme alınmış.";
                return RedirectToAction("DepoTalepleri", "Purchasing");
            }

            request.Status = "Reddedildi";
            request.IsNotified = false;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Talep reddedildi.";

            return RedirectToAction("DepoTalepleri", "Purchasing");
        }

        public IActionResult StockOut(int id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo")
            {
                return RedirectToAction("Login", "Auth");
            }

            var request = _context.Requests
                .Include(x => x.Product)
                .FirstOrDefault(x => x.Id == id);

            if (request == null || request.Product == null)
            {
                return NotFound();
            }

            if (request.Status != "Satın Alma Onayladı")
            {
                TempData["ErrorMessage"] =
                    "Sadece satın alma tarafından onaylanan talepler için stok çıkışı yapılabilir.";

                return RedirectToAction("StockOutWaiting");
            }

            if (request.Product.StockQuantity < request.Quantity)
            {
                request.Status = "Stok Yetersiz";
                request.IsNotified = true;

                _context.SaveChanges();

                TempData["ErrorMessage"] =
                    "Stok yetersiz olduğu için stok çıkışı yapılamadı.";

                return RedirectToAction("StockOutWaiting");
            }

            request.Product.StockQuantity -= request.Quantity;
            request.Status = "Stok Çıkışı Yapıldı";
            request.IsNotified = true;

            var transaction = new StockTransaction
            {
                ProductId = request.ProductId,
                TransactionType = "OUT",
                Quantity = request.Quantity,
                TransactionDate = DateTime.Now
            };

            _context.StockTransactions.Add(transaction);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Stok çıkışı yapıldı. Satın alma birimine süreç tamamlandı bildirimi gönderildi.";

            return RedirectToAction("StockOutWaiting");
        }
    }
}