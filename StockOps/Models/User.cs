using System.ComponentModel.DataAnnotations;

namespace StockOps.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public List<Request>? Requests { get; set; }

        public ICollection<StockTransaction> StockTransactions { get; set; }
            = new List<StockTransaction>();
    }
}