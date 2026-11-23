using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;
using StockOps.Services;

namespace StockOps.Controllers
{
    public class ExcelController : Controller
    {
        private readonly StockOpsDbContext _context;
        private readonly ExcelService _excelService;

        public ExcelController(StockOpsDbContext context)
        {
            _context = context;
            _excelService = new ExcelService();
        }

        [HttpGet]
        public IActionResult Import()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
                return RedirectToAction("Login", "Auth");

            var products = _context.Products.ToList();

            ViewBag.Requests = _context.Requests
                .Include(x => x.Product)
                .Include(x => x.User)
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalRequests = _context.Requests.Count();

            return View(products);
        }

        [HttpPost]
        public IActionResult Import(IFormFile file)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
                return RedirectToAction("Login", "Auth");

            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Dosya seçiniz.";

                ViewBag.Requests = _context.Requests
                    .Include(x => x.Product)
                    .Include(x => x.User)
                    .OrderByDescending(x => x.RequestDate)
                    .ToList();

                return View(_context.Products.ToList());
            }

            var safeFileName = Path.GetFileName(file.FileName);

            if (safeFileName.Contains("Talepler"))
            {
                ViewBag.Error = "Talepler Excel dosyası sisteme yüklenemez. Bu alan yalnızca ürün Excel dosyaları içindir.";

                ViewBag.Requests = _context.Requests
                    .Include(x => x.Product)
                    .Include(x => x.User)
                    .OrderByDescending(x => x.RequestDate)
                    .ToList();

                return View(_context.Products.ToList());
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads"
            );

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var products = _excelService.ReadProductsFromExcel(filePath);

            int addedCount = 0;
            int skippedCount = 0;

            foreach (var product in products)
            {
                bool exists = _context.Products.Any(x =>
                    x.Barcode == product.Barcode ||
                    x.ProductCode == product.ProductCode);

                if (!exists)
                {
                    _context.Products.Add(product);
                    addedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                addedCount + " yeni ürün eklendi. " +
                skippedCount + " ürün zaten kayıtlı olduğu için tekrar eklenmedi.";

            return RedirectToAction("Import");
        }

        public IActionResult ExportProducts()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
                return RedirectToAction("Login", "Auth");

            var products = _context.Products.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ürünler");

                worksheet.Cell(1, 1).Value = "Barkod";
                worksheet.Cell(1, 2).Value = "Ürün Kodu";
                worksheet.Cell(1, 3).Value = "Ürün Adı";
                worksheet.Cell(1, 4).Value = "Kategori";
                worksheet.Cell(1, 5).Value = "Çelik Tipi";
                worksheet.Cell(1, 6).Value = "Ölçü";
                worksheet.Cell(1, 7).Value = "Tedarikçi";
                worksheet.Cell(1, 8).Value = "Depo";
                worksheet.Cell(1, 9).Value = "Fiyat";
                worksheet.Cell(1, 10).Value = "Stok";
                worksheet.Cell(1, 11).Value = "Giriş Tarihi";
                worksheet.Cell(1, 12).Value = "Statik";

                var headerRange = worksheet.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Green;
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;

                foreach (var item in products)
                {
                    worksheet.Cell(row, 1).Value = item.Barcode;
                    worksheet.Cell(row, 2).Value = item.ProductCode;
                    worksheet.Cell(row, 3).Value = item.Name;
                    worksheet.Cell(row, 4).Value = item.Category;
                    worksheet.Cell(row, 5).Value = item.SteelType;
                    worksheet.Cell(row, 6).Value = item.Dimension;
                    worksheet.Cell(row, 7).Value = item.Supplier;
                    worksheet.Cell(row, 8).Value = item.Warehouse;
                    worksheet.Cell(row, 9).Value = item.Price;
                    worksheet.Cell(row, 10).Value = item.StockQuantity;
                    worksheet.Cell(row, 11).Value = item.ArrivalDate;
                    worksheet.Cell(row, 12).Value = item.IsStatic;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "StockOps_Urunler.xlsx"
                    );
                }
            }
        }

        public IActionResult ExportRequests()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
                return RedirectToAction("Login", "Auth");

            var requests = _context.Requests
                .Include(x => x.Product)
                .Include(x => x.User)
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Talepler");

                worksheet.Cell(1, 1).Value = "Talep No";
                worksheet.Cell(1, 2).Value = "Ürün Adı";
                worksheet.Cell(1, 3).Value = "Ürün Kodu";
                worksheet.Cell(1, 4).Value = "Barkod";
                worksheet.Cell(1, 5).Value = "Talep Eden Kullanıcı";
                worksheet.Cell(1, 6).Value = "Talep Miktarı";
                worksheet.Cell(1, 7).Value = "Mevcut Stok";
                worksheet.Cell(1, 8).Value = "Tedarikçi";
                worksheet.Cell(1, 9).Value = "Tedarikçi Telefon";
                worksheet.Cell(1, 10).Value = "Durum";
                worksheet.Cell(1, 11).Value = "Talep Tarihi";

                var headerRange = worksheet.Range(1, 1, 1, 11);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Green;
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;

                foreach (var item in requests)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Product?.Name;
                    worksheet.Cell(row, 3).Value = item.Product?.ProductCode;
                    worksheet.Cell(row, 4).Value = item.Product?.Barcode;
                    worksheet.Cell(row, 5).Value = item.User?.Username;
                    worksheet.Cell(row, 6).Value = item.Quantity;
                    worksheet.Cell(row, 7).Value = item.Product?.StockQuantity;
                    worksheet.Cell(row, 8).Value = item.Product?.Supplier;
                    worksheet.Cell(row, 9).Value = item.Product?.SupplierPhone;
                    worksheet.Cell(row, 10).Value = item.Status;
                    worksheet.Cell(row, 11).Value = item.RequestDate;

                    if (item.Status == "Stok Çıkışı Yapıldı")
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    else if (item.Status == "Reddedildi")
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightPink;
                    else if (item.Status == "Tedarik Bekliyor")
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightSalmon;
                    else if (item.Status == "Satın Alma Onayladı" || item.Status == "Onaylandı")
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    else
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightYellow;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "StockOps_Talepler.xlsx"
                    );
                }
            }
        }

        public IActionResult DownloadTemplate()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "SatinAlma")
                return RedirectToAction("Login", "Auth");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ürün Şablonu");

                string[] headers =
                {
                    "Barcode",
                    "ProductCode",
                    "Name",
                    "Category",
                    "SteelType",
                    "Dimension",
                    "Supplier",
                    "Warehouse",
                    "Price",
                    "StockQuantity",
                    "ArrivalDate",
                    "IsStatic"
                };

                for (int i = 0; i < headers.Length; i++)
                    worksheet.Cell(1, i + 1).Value = headers[i];

                worksheet.Cell(2, 1).Value = "BRK9001";
                worksheet.Cell(2, 2).Value = "PRD9001";
                worksheet.Cell(2, 3).Value = "Sıcak Haddelenmiş Sac";
                worksheet.Cell(2, 4).Value = "Sac";
                worksheet.Cell(2, 5).Value = "S355JR";
                worksheet.Cell(2, 6).Value = "2000x1000x10";
                worksheet.Cell(2, 7).Value = "Ereğli Demir Çelik";
                worksheet.Cell(2, 8).Value = "Ana Depo";
                worksheet.Cell(2, 9).Value = 1450;
                worksheet.Cell(2, 10).Value = 250;
                worksheet.Cell(2, 11).Value = DateTime.Now;
                worksheet.Cell(2, 12).Value = true;

                var headerRange = worksheet.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Green;
                headerRange.Style.Font.FontColor = XLColor.White;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "StockOps_Excel_Sablonu.xlsx"
                    );
                }
            }
        }
    }
}