using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class ForkliftMaintenance
    {
        public int Id { get; set; }

        public int ForkliftId { get; set; }

        public MaintenanceType MaintenanceType { get; set; }

        public DateTime MaintenanceDate { get; set; }
            = DateTime.Now;

        public decimal WorkingHoursAtMaintenance { get; set; }

        public decimal Cost { get; set; }

        [MaxLength(150)]
        public string? PerformedBy { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public bool IsCompleted { get; set; }

        // Navigation
        public Forklift Forklift { get; set; } = null!;
    }

    public enum MaintenanceType
    {
        PeriodicMaintenance = 1,
        Repair = 2,
        BatteryMaintenance = 3,
        TireReplacement = 4,
        SafetyInspection = 5,
        Other = 6
    }
}
