using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class RemoveAmountIsNegative : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AmountIsNegative",
            table: "Transaction",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AmountIsNegative",
            table: "Transaction");
    }
}