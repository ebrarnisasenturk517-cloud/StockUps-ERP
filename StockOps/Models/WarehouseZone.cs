using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace StockOps.Models
{
    public class WarehouseZone
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ZoneCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ZoneType ZoneType { get; set; }

        public bool IsActive { get; set; } = true;

        public Warehouse Warehouse { get; set; } = null!;

        public ICollection<StorageLocation> StorageLocations { get; set; }
            = new List<StorageLocation>();
    }

    public enum ZoneType
    {
        RawMaterial = 1,
        FinishedProduct = 2,
        SemiFinishedProduct = 3,
        Shipment = 4,
        QualityControl = 5,
        Quarantine = 6,
        Waste = 7
    }
}