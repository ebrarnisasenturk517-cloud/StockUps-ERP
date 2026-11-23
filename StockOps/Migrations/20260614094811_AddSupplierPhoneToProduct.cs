using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockOps.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPhoneToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierPhone",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierPhone",
                table: "Products");
        }
    }
}
