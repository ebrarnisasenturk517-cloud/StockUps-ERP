using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;
using StockOps.Models;

namespace StockOps.Controllers
{
    public class StorageLocationsController : Controller
    {
        private readonly StockOpsDbContext _context;

        public StorageLocationsController(StockOpsDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // Raf ve lokasyon listesi
        public IActionResult Index(int? zoneId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.StorageLocations
                .Include(x => x.WarehouseZone)
                .ThenInclude(x => x.Warehouse)
                .Include(x => x.WarehouseStocks)
                .AsQueryable();

            if (zoneId.HasValue)
            {
                query = query.Where(x =>
                    x.WarehouseZoneId == zoneId.Value);

                ViewBag.SelectedZone = _context.WarehouseZones
                    .Include(x => x.Warehouse)
                    .FirstOrDefault(x => x.Id == zoneId.Value);
            }

            var locations = query
                .OrderBy(x => x.WarehouseZone.Warehouse.Name)
                .ThenBy(x => x.WarehouseZone.Name)
                .ThenBy(x => x.LocationCode)
                .ToList();

            ViewBag.Zones = _context.WarehouseZones
                .Include(x => x.Warehouse)
                .OrderBy(x => x.Warehouse.Name)
                .ThenBy(x => x.Name)
                .ToList();

            return View(locations);
        }

        // Yeni lokasyon ekranı
        [HttpGet]
        public IActionResult Create(int? zoneId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            LoadZones(zoneId);

            var location = new StorageLocation();

            if (zoneId.HasValue)
            {
                location.WarehouseZoneId = zoneId.Value;
            }

            return View(location);
        }

        // Yeni lokasyon kaydetme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StorageLocation location)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            NormalizeLocation(location);

            if (!ValidateLocation(location))
            {
                LoadZones(location.WarehouseZoneId);
                return View(location);
            }

            bool zoneExists = _context.WarehouseZones.Any(x =>
                x.Id == location.WarehouseZoneId);

            if (!zoneExists)
            {
                ViewBag.Error = "Seçilen depo bölgesi bulunamadı.";
                LoadZones(location.WarehouseZoneId);
                return View(location);
            }

            bool codeExists = _context.StorageLocations.Any(x =>
                x.WarehouseZoneId == location.WarehouseZoneId &&
                x.LocationCode.ToLower() ==
                location.LocationCode.ToLower());

            if (codeExists)
            {
                ViewBag.Error =
                    "Bu bölgede aynı lokasyon kodu zaten bulunuyor.";

                LoadZones(location.WarehouseZoneId);
                return View(location);
            }

            location.IsActive = true;
            location.IsOccupied = false;

            _context.StorageLocations.Add(location);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Raf ve lokasyon başarıyla oluşturuldu.";

            return RedirectToAction(
                nameof(Index),
                new { zoneId = location.WarehouseZoneId });
        }

        // Lokasyon düzenleme ekranı
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var location = _context.StorageLocations
                .FirstOrDefault(x => x.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            LoadZones(location.WarehouseZoneId);

            return View(location);
        }

        // Lokasyon güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(StorageLocation location)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            NormalizeLocation(location);

            if (!ValidateLocation(location))
            {
                LoadZones(location.WarehouseZoneId);
                return View(location);
            }

            var existingLocation = _context.StorageLocations
                .FirstOrDefault(x => x.Id == location.Id);

            if (existingLocation == null)
            {
                return NotFound();
            }

            bool codeExists = _context.StorageLocations.Any(x =>
                x.Id != location.Id &&
                x.WarehouseZoneId == location.WarehouseZoneId &&
                x.LocationCode.ToLower() ==
                location.LocationCode.ToLower());

            if (codeExists)
            {
                ViewBag.Error =
                    "Bu bölgede aynı lokasyon kodu başka bir kayıtta kullanılıyor.";

                LoadZones(location.WarehouseZoneId);
                return View(location);
            }

            existingLocation.WarehouseZoneId =
                location.WarehouseZoneId;

            existingLocation.LocationCode =
                location.LocationCode;

            existingLocation.Aisle =
                location.Aisle;

            existingLocation.Rack =
                location.Rack;

            existingLocation.Shelf =
                location.Shelf;

            existingLocation.Bin =
                location.Bin;

            existingLocation.MaximumCapacity =
                location.MaximumCapacity;

            existingLocation.IsOccupied =
                location.IsOccupied;

            existingLocation.IsActive =
                location.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Raf ve lokasyon başarıyla güncellendi.";

            return RedirectToAction(
                nameof(Index),
                new { zoneId = location.WarehouseZoneId });
        }

        // Aktif/pasif yapma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var location = _context.StorageLocations
                .FirstOrDefault(x => x.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            location.IsActive = !location.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] = location.IsActive
                ? "Lokasyon aktif hale getirildi."
                : "Lokasyon pasif hale getirildi.";

            return RedirectToAction(
                nameof(Index),
                new { zoneId = location.WarehouseZoneId });
        }

        // Dolu/boş durumunu değiştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleOccupied(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var location = _context.StorageLocations
                .FirstOrDefault(x => x.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            location.IsOccupied = !location.IsOccupied;

            _context.SaveChanges();

            TempData["SuccessMessage"] = location.IsOccupied
                ? "Lokasyon dolu olarak işaretlendi."
                : "Lokasyon boş olarak işaretlendi.";

            return RedirectToAction(
                nameof(Index),
                new { zoneId = location.WarehouseZoneId });
        }

        // Lokasyon silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var location = _context.StorageLocations
                .Include(x => x.WarehouseStocks)
                .FirstOrDefault(x => x.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            if (location.WarehouseStocks.Any())
            {
                TempData["ErrorMessage"] =
                    "Bu lokasyona bağlı stok kayıtları bulunduğu için silinemez. Lokasyonu pasif hale getirebilirsiniz.";

                return RedirectToAction(
                    nameof(Index),
                    new { zoneId = location.WarehouseZoneId });
            }

            int zoneId = location.WarehouseZoneId;

            _context.StorageLocations.Remove(location);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Raf ve lokasyon başarıyla silindi.";

            return RedirectToAction(
                nameof(Index),
                new { zoneId });
        }

        private void LoadZones(int? selectedZoneId = null)
        {
            var zones = _context.WarehouseZones
                .Include(x => x.Warehouse)
                .Where(x => x.IsActive && x.Warehouse.IsActive)
                .OrderBy(x => x.Warehouse.Name)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    DisplayName =
                        x.Warehouse.Name + " - " +
                        x.ZoneCode + " - " +
                        x.Name
                })
                .ToList();

            ViewBag.ZoneList = new SelectList(
                zones,
                "Id",
                "DisplayName",
                selectedZoneId);
        }

        private static void NormalizeLocation(
            StorageLocation location)
        {
            location.LocationCode =
                (location.LocationCode ?? string.Empty).Trim();

            location.Aisle =
                string.IsNullOrWhiteSpace(location.Aisle)
                    ? null
                    : location.Aisle.Trim();

            location.Rack =
                string.IsNullOrWhiteSpace(location.Rack)
                    ? null
                    : location.Rack.Trim();

            location.Shelf =
                string.IsNullOrWhiteSpace(location.Shelf)
                    ? null
                    : location.Shelf.Trim();

            location.Bin =
                string.IsNullOrWhiteSpace(location.Bin)
                    ? null
                    : location.Bin.Trim();
        }

        private bool ValidateLocation(StorageLocation location)
        {
            if (location.WarehouseZoneId <= 0)
            {
                ViewBag.Error = "Depo bölgesi seçilmelidir.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(location.LocationCode))
            {
                ViewBag.Error =
                    "Lokasyon kodu boş bırakılamaz.";

                return false;
            }

            if (location.MaximumCapacity < 0)
            {
                ViewBag.Error =
                    "Maksimum kapasite negatif olamaz.";

                return false;
            }

            return true;
        }
    }
}