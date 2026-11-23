using StockOps.Models;

namespace StockOps.Services
{
    public class BarcodeService
    {
        public string GenerateBarcode(Product product, int serial)
        {
            return $"CLK-{product.ProductCode}-{serial:D5}";
        }
    }
}