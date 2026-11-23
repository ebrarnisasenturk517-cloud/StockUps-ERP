using Microsoft.AspNetCore.Mvc;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class AuthController : Controller
    {
        private readonly StockOpsDbContext _context;

        public AuthController(StockOpsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string username)
        {
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Username == username &&
                    x.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            TempData["SuccessMessage"] = "Sisteme başarıyla giriş yapıldı.";

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            if (user.Role == "Depo")
            {
                return RedirectToAction("Index", "Worker");
            }

            if (user.Role == "SatinAlma")
            {
                return RedirectToAction("Index", "Purchasing");
            }

            ViewBag.Error = "Kullanıcı rolü tanımlı değil.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}