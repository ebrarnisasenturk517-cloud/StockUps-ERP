using ClosedXML.Excel;
using StockOps.Models;

namespace StockOps.Services
{
    public class ExcelService
    {
        public List<Product> ReadProductsFromExcel(string filePath)
        {
            var products = new List<Product>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    decimal price = 0;
                    int stockQuantity = 0;
                    DateTime arrivalDate = DateTime.Now;
                    bool isStatic = true;

                    decimal.TryParse(row.Cell(9).GetValue<string>(), out price);
                    int.TryParse(row.Cell(10).GetValue<string>(), out stockQuantity);
                    DateTime.TryParse(row.Cell(11).GetValue<string>(), out arrivalDate);
                    bool.TryParse(row.Cell(12).GetValue<string>(), out isStatic);

                    var product = new Product
                    {
                        Barcode = row.Cell(1).GetValue<string>(),
                        ProductCode = row.Cell(2).GetValue<string>(),
                        Name = row.Cell(3).GetValue<string>(),
                        Category = row.Cell(4).GetValue<string>(),
                        SteelType = row.Cell(5).GetValue<string>(),
                        Dimension = row.Cell(6).GetValue<string>(),
                        Supplier = row.Cell(7).GetValue<string>(),
                        Warehouse = row.Cell(8).GetValue<string>(),
                        Price = price,
                        StockQuantity = stockQuantity,
                        ArrivalDate = arrivalDate,
                        IsStatic = isStatic
                    };

                    products.Add(product);
                }
            }

            return products;
        }
    }
}