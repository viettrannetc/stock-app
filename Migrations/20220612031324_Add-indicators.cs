using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class Addindicators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BandsBot",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BandsTop",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IchimokuCloudBot",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IchimokuCloudTop",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IchimokuKijun",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IchimokuTenKan",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MACDFast",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MACDMomentum",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MACDSlow",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BandsBot",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "BandsTop",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "IchimokuCloudBot",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "IchimokuCloudTop",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "IchimokuKijun",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "IchimokuTenKan",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "MACDFast",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "MACDMomentum",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "MACDSlow",
                table: "StockSymbolHistory");
        }
    }
}
