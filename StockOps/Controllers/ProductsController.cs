using Microsoft.AspNetCore.Mvc;
using StockOps.Data;
using StockOps.Models;
using StockOps.Services;

namespace StockOps.Controllers
{
    public class ProductsController : Controller
    {
        private readonly StockOpsDbContext _context;
        private readonly BarcodeService _barcodeService;

        public ProductsController(StockOpsDbContext context)
        {
            _context = context;
            _barcodeService = new BarcodeService();
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult CriticalStock()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Depo" && role != "SatinAlma")
            {
                return RedirectToAction("Login", "Auth");
            }

            var products = _context.Products
                .Where(x => x.StockQuantity < 20)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (product.ArrivalDate == DateTime.MinValue)
            {
                product.ArrivalDate = DateTime.Now;
            }

            int serialNumber = _context.Products.Count() + 1;

            product.Barcode = _barcodeService.GenerateBarcode(product, serialNumber);

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Ürün başarıyla eklendi.";

            return RedirectToAction("Index");
        }
    }
}