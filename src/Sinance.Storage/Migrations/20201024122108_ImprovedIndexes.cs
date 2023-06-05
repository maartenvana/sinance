using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class ImprovedIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ShortName",
            table: "Category",
            maxLength: 4,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(3)",
            oldMaxLength: 3,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "BankAccount",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext");

        migrationBuilder.CreateIndex(
            name: "IX_Category_Name",
            table: "Category",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Category_ShortName",
            table: "Category",
            column: "ShortName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BankAccount_Name",
            table: "BankAccount",
            column: "Name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Category_Name",
            table: "Category");

        migrationBuilder.DropIndex(
            name: "IX_Category_ShortName",
            table: "Category");

        migrationBuilder.DropIndex(
            name: "IX_BankAccount_Name",
            table: "BankAccount");

        migrationBuilder.AlterColumn<string>(
            name: "ShortName",
            table: "Category",
            type: "varchar(3)",
            maxLength: 3,
            nullable: true,
            oldClrType: typeof(string),
            oldMaxLength: 4,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "BankAccount",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string));
    }
}
