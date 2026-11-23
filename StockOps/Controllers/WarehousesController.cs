using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;
using StockOps.Models;

namespace StockOps.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly StockOpsDbContext _context;

        public WarehousesController(StockOpsDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");

            return role == "Admin";
        }

        // Depo listesi
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouses = _context.Warehouses
                .OrderBy(x => x.Name)
                .ToList();

            return View(warehouses);
        }

        // Depo ekleme sayfası
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(new Warehouse());
        }

        // Depo ekleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Warehouse warehouse)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            warehouse.WarehouseCode =
                (warehouse.WarehouseCode ?? string.Empty).Trim();

            warehouse.Name =
                (warehouse.Name ?? string.Empty).Trim();

            warehouse.Location =
                (warehouse.Location ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(warehouse.WarehouseCode))
            {
                ViewBag.Error = "Depo kodu boş bırakılamaz.";
                return View(warehouse);
            }

            if (string.IsNullOrWhiteSpace(warehouse.Name))
            {
                ViewBag.Error = "Depo adı boş bırakılamaz.";
                return View(warehouse);
            }

            if (string.IsNullOrWhiteSpace(warehouse.Location))
            {
                ViewBag.Error = "Depo konumu boş bırakılamaz.";
                return View(warehouse);
            }

            if (warehouse.TotalCapacity < 0)
            {
                ViewBag.Error = "Toplam kapasite negatif olamaz.";
                return View(warehouse);
            }

            bool codeExists = _context.Warehouses.Any(x =>
                x.WarehouseCode.ToLower() ==
                warehouse.WarehouseCode.ToLower());

            if (codeExists)
            {
                ViewBag.Error = "Bu depo kodu daha önce kullanılmış.";
                return View(warehouse);
            }

            warehouse.IsActive = true;

            _context.Warehouses.Add(warehouse);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Depo başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        // Depo düzenleme sayfası
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = _context.Warehouses
                .FirstOrDefault(x => x.Id == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }

        // Depo düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Warehouse warehouse)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            warehouse.WarehouseCode =
                (warehouse.WarehouseCode ?? string.Empty).Trim();

            warehouse.Name =
                (warehouse.Name ?? string.Empty).Trim();

            warehouse.Location =
                (warehouse.Location ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(warehouse.WarehouseCode))
            {
                ViewBag.Error = "Depo kodu boş bırakılamaz.";
                return View(warehouse);
            }

            if (string.IsNullOrWhiteSpace(warehouse.Name))
            {
                ViewBag.Error = "Depo adı boş bırakılamaz.";
                return View(warehouse);
            }

            if (string.IsNullOrWhiteSpace(warehouse.Location))
            {
                ViewBag.Error = "Depo konumu boş bırakılamaz.";
                return View(warehouse);
            }

            if (warehouse.TotalCapacity < 0)
            {
                ViewBag.Error = "Toplam kapasite negatif olamaz.";
                return View(warehouse);
            }

            var existingWarehouse = _context.Warehouses
                .FirstOrDefault(x => x.Id == warehouse.Id);

            if (existingWarehouse == null)
            {
                return NotFound();
            }

            bool codeExists = _context.Warehouses.Any(x =>
                x.Id != warehouse.Id &&
                x.WarehouseCode.ToLower() ==
                warehouse.WarehouseCode.ToLower());

            if (codeExists)
            {
                ViewBag.Error =
                    "Bu depo kodu başka bir depoda kullanılıyor.";

                return View(warehouse);
            }

            existingWarehouse.WarehouseCode =
                warehouse.WarehouseCode;

            existingWarehouse.Name =
                warehouse.Name;

            existingWarehouse.Location =
                warehouse.Location;

            existingWarehouse.TotalCapacity =
                warehouse.TotalCapacity;

            existingWarehouse.IsActive =
                warehouse.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Depo başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        // Depo detayları
        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = _context.Warehouses
                .Include(x => x.WarehouseZones)
                .Include(x => x.Forklifts)
                .Include(x => x.WarehouseStocks)
                .ThenInclude(x => x.Product)
                .FirstOrDefault(x => x.Id == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }

        // Depoyu aktif/pasif yap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = _context.Warehouses
                .FirstOrDefault(x => x.Id == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            warehouse.IsActive = !warehouse.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                warehouse.IsActive
                    ? "Depo aktif hale getirildi."
                    : "Depo pasif hale getirildi.";

            return RedirectToAction(nameof(Index));
        }

        // Depo silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = _context.Warehouses
                .Include(x => x.WarehouseStocks)
                .Include(x => x.StockTransactions)
                .Include(x => x.WarehouseZones)
                .Include(x => x.Forklifts)
                .FirstOrDefault(x => x.Id == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            bool hasRelatedRecords =
                warehouse.WarehouseStocks.Any() ||
                warehouse.StockTransactions.Any() ||
                warehouse.WarehouseZones.Any() ||
                warehouse.Forklifts.Any();

            if (hasRelatedRecords)
            {
                TempData["ErrorMessage"] =
                    "Bu depoya bağlı stok, hareket, bölge veya forklift bulunduğu için depo silinemez. Depoyu pasif hale getirebilirsiniz.";

                return RedirectToAction(nameof(Index));
            }

            _context.Warehouses.Remove(warehouse);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Depo başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}