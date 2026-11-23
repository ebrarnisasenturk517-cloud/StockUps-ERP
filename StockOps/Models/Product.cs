using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Barcode { get; set; } = string.Empty;

        public string ProductCode { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Supplier { get; set; } = string.Empty;

        public string SupplierPhone { get; set; } = string.Empty;

        public string Warehouse { get; set; } = string.Empty;

        public int StockQuantity { get; set; }

        public decimal Price { get; set; }

        public string SteelType { get; set; } = string.Empty;

        public string Dimension { get; set; } = string.Empty;

        public DateTime ArrivalDate { get; set; } = DateTime.Now;

        public bool IsStatic { get; set; }

        public List<Request>? Requests { get; set; }

        public List<StockTransaction>? StockTransactions { get; set; }

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
            = new List<WarehouseStock>();
    }
}