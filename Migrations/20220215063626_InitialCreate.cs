using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockSymbol",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    _sc_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    _bp_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _clp_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _fp_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _op_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _cp_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _lp_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    change = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _pc_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _tvol_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _tval_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _vhtt_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    _in_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    _sin_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    catID = table.Column<int>(type: "int", nullable: false),
                    stockName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    _diviend_ = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbol", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbolHistory",
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
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolHistory", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Todo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Todo", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockSymbol");

            migrationBuilder.DropTable(
                name: "StockSymbolHistory");

            migrationBuilder.DropTable(
                name: "Todo");
        }
    }
}
