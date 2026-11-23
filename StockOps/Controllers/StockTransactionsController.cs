using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOps.Data;

namespace StockOps.Controllers
{
    public class StockTransactionsController : Controller
    {
        private readonly StockOpsDbContext _context;

        public StockTransactionsController(StockOpsDbContext context)
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

            var transactions = _context.StockTransactions
                .Include(x => x.Product)
                .OrderByDescending(x => x.TransactionDate)
                .ToList();

            return View(transactions);
        }
    }
}