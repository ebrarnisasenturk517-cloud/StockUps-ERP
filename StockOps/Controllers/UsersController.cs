using Microsoft.AspNetCore.Mvc;
using StockOps.Data;
using StockOps.Models;

namespace StockOps.Controllers
{
    public class UsersController : Controller
    {
        private readonly StockOpsDbContext _context;

        public UsersController(StockOpsDbContext context)
        {
            _context = context;
        }

        // Sadece Admin kullanıcılar erişebilir.
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");

            return role == "Admin";
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var users = _context.Users
                .OrderBy(x => x.Username)
                .ToList();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            user.Username = (user.Username ?? string.Empty).Trim();
            user.Password = (user.Password ?? string.Empty).Trim();
            user.Role = (user.Role ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                ViewBag.Error = "Kullanıcı adı boş bırakılamaz.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ViewBag.Error = "Şifre boş bırakılamaz.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Role))
            {
                ViewBag.Error = "Rol seçilmelidir.";
                return View(user);
            }

            var normalizedUsername = user.Username.ToLower();

            bool exists = _context.Users.Any(x =>
                x.Username.ToLower() == normalizedUsername);

            if (exists)
            {
                ViewBag.Error = "Bu kullanıcı adı zaten kayıtlı.";
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _context.Users
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User user)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            user.Username = (user.Username ?? string.Empty).Trim();
            user.Password = (user.Password ?? string.Empty).Trim();
            user.Role = (user.Role ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                ViewBag.Error = "Kullanıcı adı boş bırakılamaz.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ViewBag.Error = "Şifre boş bırakılamaz.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Role))
            {
                ViewBag.Error = "Rol seçilmelidir.";
                return View(user);
            }

            var existingUser = _context.Users
                .FirstOrDefault(x => x.Id == user.Id);

            if (existingUser == null)
            {
                return NotFound();
            }

            var normalizedUsername = user.Username.ToLower();

            bool usernameExists = _context.Users.Any(x =>
                x.Id != user.Id &&
                x.Username.ToLower() == normalizedUsername);

            if (usernameExists)
            {
                ViewBag.Error =
                    "Bu kullanıcı adı başka bir kullanıcıda kayıtlı.";

                return View(user);
            }

            existingUser.Username = user.Username;
            existingUser.Password = user.Password;
            existingUser.Role = user.Role;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _context.Users
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Giriş yapan yönetici kendi hesabını silemesin.
            var currentUsername =
                HttpContext.Session.GetString("Username");

            if (!string.IsNullOrWhiteSpace(currentUsername) &&
                user.Username.Equals(
                    currentUsername,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "Giriş yaptığınız kullanıcı hesabını silemezsiniz.";

                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}