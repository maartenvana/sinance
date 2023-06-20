using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class RemoveUnusedUserIds : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "TransactionCategory",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "ImportMapping",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "ImportBank",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "CustomReportCategory",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_TransactionCategory_UserId",
            table: "TransactionCategory",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ImportMapping_UserId",
            table: "ImportMapping",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ImportBank_UserId",
            table: "ImportBank",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_CustomReportCategory_UserId",
            table: "CustomReportCategory",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReportCategory_Users_UserId",
            table: "CustomReportCategory",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ImportBank_Users_UserId",
            table: "ImportBank",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ImportMapping_Users_UserId",
            table: "ImportMapping",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TransactionCategory_Users_UserId",
            table: "TransactionCategory",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CustomReportCategory_Users_UserId",
            table: "CustomReportCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_ImportBank_Users_UserId",
            table: "ImportBank");

        migrationBuilder.DropForeignKey(
            name: "FK_ImportMapping_Users_UserId",
            table: "ImportMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_TransactionCategory_Users_UserId",
            table: "TransactionCategory");

        migrationBuilder.DropIndex(
            name: "IX_TransactionCategory_UserId",
            table: "TransactionCategory");

        migrationBuilder.DropIndex(
            name: "IX_ImportMapping_UserId",
            table: "ImportMapping");

        migrationBuilder.DropIndex(
            name: "IX_ImportBank_UserId",
            table: "ImportBank");

        migrationBuilder.DropIndex(
            name: "IX_CustomReportCategory_UserId",
            table: "CustomReportCategory");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "TransactionCategory");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "ImportMapping");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "ImportBank");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "CustomReportCategory");
    }
}