using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;

        public Warehouse Warehouse { get; set; } = null!;

        public User? User { get; set; }
    }
}