using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations;

public partial class RemovedProfitLossInclusionOption : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IncludeInProfitLossGraph",
            table: "BankAccount");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IncludeInProfitLossGraph",
            table: "BankAccount",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }
}
