using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class WarehouseStock
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public int? StorageLocationId { get; set; }

        public decimal Quantity { get; set; }

        public decimal ReservedQuantity { get; set; }

        public decimal MinimumStockLevel { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        public Product Product { get; set; } = null!;

        public Warehouse Warehouse { get; set; } = null!;

        public StorageLocation? StorageLocation { get; set; }
    }
}