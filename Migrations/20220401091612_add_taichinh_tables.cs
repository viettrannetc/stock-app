using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class add_taichinh_tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockSymbolCDKT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockSymbolFinanceHistoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolCDKT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbolCSTC",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockSymbolFinanceHistoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolCSTC", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbolCTKH",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockSymbolFinanceHistoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolCTKH", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbolFinanceHistory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearPeriod = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: false),
                    TermCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodBegin = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolFinanceHistory", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbolKQKD",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockSymbolFinanceHistoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolKQKD", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockSymbolLCTT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockSymbolFinanceHistoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportComponentNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSymbolLCTT", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockSymbolCDKT");

            migrationBuilder.DropTable(
                name: "StockSymbolCSTC");

            migrationBuilder.DropTable(
                name: "StockSymbolCTKH");

            migrationBuilder.DropTable(
                name: "StockSymbolFinanceHistory");

            migrationBuilder.DropTable(
                name: "StockSymbolKQKD");

            migrationBuilder.DropTable(
                name: "StockSymbolLCTT");
        }
    }
}
