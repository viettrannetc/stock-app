using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    public partial class addPKAForHours : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MACDPhanKi",
                table: "HistoryHour",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RSIPhanKi",
                table: "HistoryHour",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MACDPhanKi",
                table: "HistoryHour");

            migrationBuilder.DropColumn(
                name: "RSIPhanKi",
                table: "HistoryHour");
        }
    }
}
