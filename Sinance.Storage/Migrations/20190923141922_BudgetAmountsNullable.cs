using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations
{
    public partial class BudgetAmountsNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Budget",
                nullable: true,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Budget",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }
    }
}
