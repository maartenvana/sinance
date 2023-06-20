using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class SinanceUserIdGeneration : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Users",
            nullable: false,
            oldClrType: typeof(int))
            .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Users",
            nullable: false,
            oldClrType: typeof(int))
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
    }
}