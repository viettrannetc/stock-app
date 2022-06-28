using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class AddHourData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryHour",
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
                    BandsMid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MACD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MACDSignal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MACDMomentum = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuTenKan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuKijun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuCloudTop = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IchimokuCloudBot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaMA05 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryHour", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryHour");
        }
    }
}
