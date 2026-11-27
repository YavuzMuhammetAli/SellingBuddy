using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SellersAndShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_ContactName",
                schema: "ordering",
                table: "orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Phone",
                schema: "ordering",
                table: "orders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                schema: "ordering",
                table: "orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethod",
                schema: "ordering",
                table: "orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSlug",
                schema: "ordering",
                table: "orderItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                schema: "ordering",
                table: "orderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerName",
                schema: "ordering",
                table: "orderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_ContactName",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "Address_Phone",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ProductSlug",
                schema: "ordering",
                table: "orderItems");

            migrationBuilder.DropColumn(
                name: "SellerId",
                schema: "ordering",
                table: "orderItems");

            migrationBuilder.DropColumn(
                name: "SellerName",
                schema: "ordering",
                table: "orderItems");
        }
    }
}
