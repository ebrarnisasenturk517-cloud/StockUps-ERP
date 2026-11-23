namespace StockOps.Models
{
    public class Request
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        public int Quantity { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Bekliyor";

        public bool IsNotified { get; set; } = false;

        public Product? Product { get; set; }

        public User? User { get; set; }
    }
}