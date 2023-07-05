using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinance.Storage.Migrations;

public partial class SourceTransactions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_BankAccount_BankAccountId",
            table: "Transaction");

        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_Category_CategoryId",
            table: "Transaction");

        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_Users_UserId",
            table: "Transaction");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Transaction",
            table: "Transaction");

        migrationBuilder.RenameTable(
            name: "Transaction",
            newName: "SourceTransactions");

        migrationBuilder.RenameIndex(
            name: "IX_Transaction_UserId",
            table: "SourceTransactions",
            newName: "IX_SourceTransactions_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_Transaction_CategoryId",
            table: "SourceTransactions",
            newName: "IX_SourceTransactions_CategoryId");

        migrationBuilder.RenameIndex(
            name: "IX_Transaction_BankAccountId",
            table: "SourceTransactions",
            newName: "IX_SourceTransactions_BankAccountId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_SourceTransactions",
            table: "SourceTransactions",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_SourceTransactions_BankAccount_BankAccountId",
            table: "SourceTransactions",
            column: "BankAccountId",
            principalTable: "BankAccount",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_SourceTransactions_Category_CategoryId",
            table: "SourceTransactions",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_SourceTransactions_Users_UserId",
            table: "SourceTransactions",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SourceTransactions_BankAccount_BankAccountId",
            table: "SourceTransactions");

        migrationBuilder.DropForeignKey(
            name: "FK_SourceTransactions_Category_CategoryId",
            table: "SourceTransactions");

        migrationBuilder.DropForeignKey(
            name: "FK_SourceTransactions_Users_UserId",
            table: "SourceTransactions");

        migrationBuilder.DropPrimaryKey(
            name: "PK_SourceTransactions",
            table: "SourceTransactions");

        migrationBuilder.RenameTable(
            name: "SourceTransactions",
            newName: "Transaction");

        migrationBuilder.RenameIndex(
            name: "IX_SourceTransactions_UserId",
            table: "Transaction",
            newName: "IX_Transaction_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_SourceTransactions_CategoryId",
            table: "Transaction",
            newName: "IX_Transaction_CategoryId");

        migrationBuilder.RenameIndex(
            name: "IX_SourceTransactions_BankAccountId",
            table: "Transaction",
            newName: "IX_Transaction_BankAccountId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Transaction",
            table: "Transaction",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_BankAccount_BankAccountId",
            table: "Transaction",
            column: "BankAccountId",
            principalTable: "BankAccount",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_Category_CategoryId",
            table: "Transaction",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_Users_UserId",
            table: "Transaction",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
