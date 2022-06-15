using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class UpdateTableHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Todo");

            migrationBuilder.DropColumn(
                name: "MACDFast",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "MACDSlow",
                table: "StockSymbolHistory");

            migrationBuilder.DropColumn(
                name: "PE",
                table: "StockSymbolHistory");

            migrationBuilder.RenameColumn(
                name: "RSIAvgL",
                table: "StockSymbolHistory",
                newName: "MACDSignal");

            migrationBuilder.RenameColumn(
                name: "RSIAvgG",
                table: "StockSymbolHistory",
                newName: "MACD");

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    O = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    C = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    L = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    H = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    V = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    T = table.Column<int>(type: "int", nullable: false),
                    RSI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NenTop = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NenBot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BandsTop = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BandsBot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MACD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MACDSignal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MACDMomentum = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuTenKan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuKijun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuCloudTop = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuCloudBot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.RenameColumn(
                name: "MACDSignal",
                table: "StockSymbolHistory",
                newName: "RSIAvgL");

            migrationBuilder.RenameColumn(
                name: "MACD",
                table: "StockSymbolHistory",
                newName: "RSIAvgG");

            migrationBuilder.AddColumn<decimal>(
                name: "MACDFast",
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

            migrationBuilder.AddColumn<decimal>(
                name: "PE",
                table: "StockSymbolHistory",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Todo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Todo", x => x.ID);
                });
        }
    }
}
