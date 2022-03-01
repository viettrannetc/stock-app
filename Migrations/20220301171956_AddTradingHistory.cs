using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class AddTradingHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.CreateTable(
                name: "StockSymbolTradingHistory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Change = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MatchQtty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVol = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsBuy = table.Column<bool>(type: "bit", nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolTradingHistory", x => x.ID);
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockSymbolTradingHistory");
        }
    }
}
