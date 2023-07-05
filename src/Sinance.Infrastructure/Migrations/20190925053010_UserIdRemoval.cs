using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations;

public partial class UserIdRemoval : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "UserId",
            table: "Users",
            nullable: true);
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UserId",
            table: "Users");
    }
}