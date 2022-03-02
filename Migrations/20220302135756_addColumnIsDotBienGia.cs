using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class addColumnIsDotBienGia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTangDotBien",
                table: "StockSymbolTradingHistory",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTangDotBien",
                table: "StockSymbolTradingHistory");
        }
    }
}
