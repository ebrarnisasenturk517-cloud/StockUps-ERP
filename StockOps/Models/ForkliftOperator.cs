using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class ForkliftOperator
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public DateTime? LicenseExpiryDate { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public ICollection<Forklift> Forklifts { get; set; }
            = new List<Forklift>();
    }
}