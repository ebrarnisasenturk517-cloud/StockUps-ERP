using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;
using StockOps.Models;

namespace StockOps.Controllers
{
    public class WarehouseZonesController : Controller
    {
        private readonly StockOpsDbContext _context;

        public WarehouseZonesController(StockOpsDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public IActionResult Index(int? warehouseId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.WarehouseZones
                .Include(x => x.Warehouse)
                .Include(x => x.StorageLocations)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(x => x.WarehouseId == warehouseId.Value);

                ViewBag.SelectedWarehouse = _context.Warehouses
                    .FirstOrDefault(x => x.Id == warehouseId.Value);
            }

            var zones = query
                .OrderBy(x => x.Warehouse.Name)
                .ThenBy(x => x.ZoneCode)
                .ToList();

            ViewBag.Warehouses = _context.Warehouses
                .OrderBy(x => x.Name)
                .ToList();

            return View(zones);
        }

        [HttpGet]
        public IActionResult Create(int? warehouseId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            LoadWarehouses(warehouseId);

            var zone = new WarehouseZone();

            if (warehouseId.HasValue)
            {
                zone.WarehouseId = warehouseId.Value;
            }

            return View(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(WarehouseZone zone)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            zone.ZoneCode = (zone.ZoneCode ?? string.Empty).Trim();
            zone.Name = (zone.Name ?? string.Empty).Trim();

            if (zone.WarehouseId <= 0)
            {
                ViewBag.Error = "Depo seçilmelidir.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            if (string.IsNullOrWhiteSpace(zone.ZoneCode))
            {
                ViewBag.Error = "Bölge kodu boş bırakılamaz.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            if (string.IsNullOrWhiteSpace(zone.Name))
            {
                ViewBag.Error = "Bölge adı boş bırakılamaz.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            bool warehouseExists = _context.Warehouses
                .Any(x => x.Id == zone.WarehouseId);

            if (!warehouseExists)
            {
                ViewBag.Error = "Seçilen depo bulunamadı.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            bool codeExists = _context.WarehouseZones.Any(x =>
                x.WarehouseId == zone.WarehouseId &&
                x.ZoneCode.ToLower() == zone.ZoneCode.ToLower());

            if (codeExists)
            {
                ViewBag.Error =
                    "Bu bölgede aynı kodla başka bir kayıt bulunuyor.";

                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            zone.IsActive = true;

            _context.WarehouseZones.Add(zone);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Depo bölgesi başarıyla oluşturuldu.";

            return RedirectToAction(
                nameof(Index),
                new { warehouseId = zone.WarehouseId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var zone = _context.WarehouseZones
                .FirstOrDefault(x => x.Id == id);

            if (zone == null)
            {
                return NotFound();
            }

            LoadWarehouses(zone.WarehouseId);

            return View(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(WarehouseZone zone)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            zone.ZoneCode = (zone.ZoneCode ?? string.Empty).Trim();
            zone.Name = (zone.Name ?? string.Empty).Trim();

            if (zone.WarehouseId <= 0)
            {
                ViewBag.Error = "Depo seçilmelidir.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            if (string.IsNullOrWhiteSpace(zone.ZoneCode))
            {
                ViewBag.Error = "Bölge kodu boş bırakılamaz.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            if (string.IsNullOrWhiteSpace(zone.Name))
            {
                ViewBag.Error = "Bölge adı boş bırakılamaz.";
                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            var existingZone = _context.WarehouseZones
                .FirstOrDefault(x => x.Id == zone.Id);

            if (existingZone == null)
            {
                return NotFound();
            }

            bool codeExists = _context.WarehouseZones.Any(x =>
                x.Id != zone.Id &&
                x.WarehouseId == zone.WarehouseId &&
                x.ZoneCode.ToLower() == zone.ZoneCode.ToLower());

            if (codeExists)
            {
                ViewBag.Error =
                    "Bu depoda aynı kodla başka bir bölge bulunuyor.";

                LoadWarehouses(zone.WarehouseId);
                return View(zone);
            }

            existingZone.WarehouseId = zone.WarehouseId;
            existingZone.ZoneCode = zone.ZoneCode;
            existingZone.Name = zone.Name;
            existingZone.ZoneType = zone.ZoneType;
            existingZone.IsActive = zone.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Depo bölgesi başarıyla güncellendi.";

            return RedirectToAction(
                nameof(Index),
                new { warehouseId = zone.WarehouseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var zone = _context.WarehouseZones
                .FirstOrDefault(x => x.Id == id);

            if (zone == null)
            {
                return NotFound();
            }

            zone.IsActive = !zone.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] = zone.IsActive
                ? "Depo bölgesi aktif hale getirildi."
                : "Depo bölgesi pasif hale getirildi.";

            return RedirectToAction(
                nameof(Index),
                new { warehouseId = zone.WarehouseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var zone = _context.WarehouseZones
                .Include(x => x.StorageLocations)
                .FirstOrDefault(x => x.Id == id);

            if (zone == null)
            {
                return NotFound();
            }

            if (zone.StorageLocations.Any())
            {
                TempData["ErrorMessage"] =
                    "Bu bölgeye bağlı raf veya lokasyonlar bulunduğu için silinemez. Önce bölgeyi pasif hale getirebilirsiniz.";

                return RedirectToAction(
                    nameof(Index),
                    new { warehouseId = zone.WarehouseId });
            }

            int warehouseId = zone.WarehouseId;

            _context.WarehouseZones.Remove(zone);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Depo bölgesi başarıyla silindi.";

            return RedirectToAction(
                nameof(Index),
                new { warehouseId });
        }

        private void LoadWarehouses(int? selectedWarehouseId = null)
        {
            var warehouses = _context.Warehouses
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.WarehouseList = new SelectList(
                warehouses,
                "Id",
                "Name",
                selectedWarehouseId);
        }
    }
}