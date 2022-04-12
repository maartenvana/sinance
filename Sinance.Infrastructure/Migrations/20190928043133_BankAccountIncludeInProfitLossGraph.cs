using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations
{
    public partial class BankAccountIncludeInProfitLossGraph : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeInProfitLossGraph",
                table: "BankAccount");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeInProfitLossGraph",
                table: "BankAccount",
                nullable: false,
                defaultValue: true);
        }
    }
}