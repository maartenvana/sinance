using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations;

public partial class StandardImportBanks : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsStandard",
            table: "ImportBank");
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsStandard",
            table: "ImportBank",
            nullable: false,
            defaultValue: false);
    }
}