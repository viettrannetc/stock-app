using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class add_KLGD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KLGDMuaBan",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalBuy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSell = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalUnknow = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVol = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KLGDMuaBan", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KLGDMuaBan");
        }
    }
}
