using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class StorageLocation
    {
        public int Id { get; set; }

        public int WarehouseZoneId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Aisle { get; set; }

        [MaxLength(50)]
        public string? Rack { get; set; }

        [MaxLength(50)]
        public string? Shelf { get; set; }

        [MaxLength(50)]
        public string? Bin { get; set; }

        public decimal MaximumCapacity { get; set; }

        public bool IsOccupied { get; set; }

        public bool IsActive { get; set; } = true;

        public WarehouseZone WarehouseZone { get; set; } = null!;

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
            = new List<WarehouseStock>();
    }
}