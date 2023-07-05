using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class CascadingDeletes : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_BankAccount_Users_UserId",
            table: "BankAccount");

        migrationBuilder.DropForeignKey(
            name: "FK_Category_Users_UserId",
            table: "Category");

        migrationBuilder.DropForeignKey(
            name: "FK_CategoryMapping_Category_CategoryId",
            table: "CategoryMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_CategoryMapping_Users_UserId",
            table: "CategoryMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_CustomReport_Users_UserId",
            table: "CustomReport");

        migrationBuilder.DropForeignKey(
            name: "FK_CustomReportCategory_Category_CategoryId",
            table: "CustomReportCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_CustomReportCategory_CustomReport_CustomReportId",
            table: "CustomReportCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_ImportMapping_ImportBank_ImportBankId",
            table: "ImportMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_BankAccount_BankAccountId",
            table: "Transaction");

        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_Users_UserId",
            table: "Transaction");

        migrationBuilder.DropForeignKey(
            name: "FK_TransactionCategory_Category_CategoryId",
            table: "TransactionCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_TransactionCategory_Transaction_TransactionId",
            table: "TransactionCategory");

        migrationBuilder.AddForeignKey(
            name: "FK_BankAccount_Users_UserId",
            table: "BankAccount",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Category_Users_UserId",
            table: "Category",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CategoryMapping_Category_CategoryId",
            table: "CategoryMapping",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CategoryMapping_Users_UserId",
            table: "CategoryMapping",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReport_Users_UserId",
            table: "CustomReport",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReportCategory_Category_CategoryId",
            table: "CustomReportCategory",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReportCategory_CustomReport_CustomReportId",
            table: "CustomReportCategory",
            column: "CustomReportId",
            principalTable: "CustomReport",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ImportMapping_ImportBank_ImportBankId",
            table: "ImportMapping",
            column: "ImportBankId",
            principalTable: "ImportBank",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_BankAccount_BankAccountId",
            table: "Transaction",
            column: "BankAccountId",
            principalTable: "BankAccount",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_Users_UserId",
            table: "Transaction",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TransactionCategory_Category_CategoryId",
            table: "TransactionCategory",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TransactionCategory_Transaction_TransactionId",
            table: "TransactionCategory",
            column: "TransactionId",
            principalTable: "Transaction",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_BankAccount_Users_UserId",
            table: "BankAccount");

        migrationBuilder.DropForeignKey(
            name: "FK_Category_Users_UserId",
            table: "Category");

        migrationBuilder.DropForeignKey(
            name: "FK_CategoryMapping_Category_CategoryId",
            table: "CategoryMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_CategoryMapping_Users_UserId",
            table: "CategoryMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_CustomReport_Users_UserId",
            table: "CustomReport");

        migrationBuilder.DropForeignKey(
            name: "FK_CustomReportCategory_Category_CategoryId",
            table: "CustomReportCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_CustomReportCategory_CustomReport_CustomReportId",
            table: "CustomReportCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_ImportMapping_ImportBank_ImportBankId",
            table: "ImportMapping");

        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_BankAccount_BankAccountId",
            table: "Transaction");

        migrationBuilder.DropForeignKey(
            name: "FK_Transaction_Users_UserId",
            table: "Transaction");

        migrationBuilder.DropForeignKey(
            name: "FK_TransactionCategory_Category_CategoryId",
            table: "TransactionCategory");

        migrationBuilder.DropForeignKey(
            name: "FK_TransactionCategory_Transaction_TransactionId",
            table: "TransactionCategory");

        migrationBuilder.AddForeignKey(
            name: "FK_BankAccount_Users_UserId",
            table: "BankAccount",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Category_Users_UserId",
            table: "Category",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CategoryMapping_Category_CategoryId",
            table: "CategoryMapping",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CategoryMapping_Users_UserId",
            table: "CategoryMapping",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReport_Users_UserId",
            table: "CustomReport",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReportCategory_Category_CategoryId",
            table: "CustomReportCategory",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CustomReportCategory_CustomReport_CustomReportId",
            table: "CustomReportCategory",
            column: "CustomReportId",
            principalTable: "CustomReport",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ImportMapping_ImportBank_ImportBankId",
            table: "ImportMapping",
            column: "ImportBankId",
            principalTable: "ImportBank",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_BankAccount_BankAccountId",
            table: "Transaction",
            column: "BankAccountId",
            principalTable: "BankAccount",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Transaction_Users_UserId",
            table: "Transaction",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TransactionCategory_Category_CategoryId",
            table: "TransactionCategory",
            column: "CategoryId",
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TransactionCategory_Transaction_TransactionId",
            table: "TransactionCategory",
            column: "TransactionId",
            principalTable: "Transaction",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}