using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class Forklift
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ForkliftCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        public decimal CarryingCapacityKg { get; set; }

        public decimal TotalWorkingHours { get; set; }

        public ForkliftStatus Status { get; set; }
            = ForkliftStatus.Available;

        public DateTime? LastMaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public int WarehouseId { get; set; }

        public int? AssignedOperatorId { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public Warehouse Warehouse { get; set; } = null!;

        public ForkliftOperator? AssignedOperator { get; set; }

        public ICollection<ForkliftMaintenance> MaintenanceRecords { get; set; }
            = new List<ForkliftMaintenance>();
    }

    public enum ForkliftStatus
    {
        Available = 1,
        InUse = 2,
        UnderMaintenance = 3,
        Faulty = 4,
        OutOfService = 5
    }
}