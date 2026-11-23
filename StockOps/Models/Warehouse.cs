using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string WarehouseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Location { get; set; } = string.Empty;

        public decimal TotalCapacity { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<WarehouseZone> WarehouseZones { get; set; }
            = new List<WarehouseZone>();

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
            = new List<WarehouseStock>();

        public ICollection<StockTransaction> StockTransactions { get; set; }
            = new List<StockTransaction>();

        public ICollection<Forklift> Forklifts { get; set; }
            = new List<Forklift>();
    }
}