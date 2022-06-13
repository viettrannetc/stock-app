using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class AddCalculationRSIColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BiChanGiaoDich",
                table: "StockSymbol",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MA20Vol",
                table: "StockSymbol",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiChanGiaoDich",
                table: "StockSymbol");

            migrationBuilder.DropColumn(
                name: "MA20Vol",
                table: "StockSymbol");
        }
    }
}
