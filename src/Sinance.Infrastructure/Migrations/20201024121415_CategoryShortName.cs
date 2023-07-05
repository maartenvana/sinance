using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations;

public partial class CategoryShortName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ShortName",
            table: "Category",
            maxLength: 3,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ShortName",
            table: "Category");
    }
}
