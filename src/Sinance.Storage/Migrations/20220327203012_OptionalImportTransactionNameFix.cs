using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinance.Storage.Migrations;

public partial class OptionalImportTransactionNameFix : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Transactions_ImportTransactions_SourceTransactionId",
            table: "Transactions");

        migrationBuilder.RenameColumn(
            name: "SourceTransactionId",
            table: "Transactions",
            newName: "ImportTransactionId");

        migrationBuilder.RenameIndex(
            name: "IX_Transactions_SourceTransactionId",
            table: "Transactions",
            newName: "IX_Transactions_ImportTransactionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Transactions_ImportTransactions_ImportTransactionId",
            table: "Transactions",
            column: "ImportTransactionId",
            principalTable: "ImportTransactions",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Transactions_ImportTransactions_ImportTransactionId",
            table: "Transactions");

        migrationBuilder.RenameColumn(
            name: "ImportTransactionId",
            table: "Transactions",
            newName: "SourceTransactionId");

        migrationBuilder.RenameIndex(
            name: "IX_Transactions_ImportTransactionId",
            table: "Transactions",
            newName: "IX_Transactions_SourceTransactionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Transactions_ImportTransactions_SourceTransactionId",
            table: "Transactions",
            column: "SourceTransactionId",
            principalTable: "ImportTransactions",
            principalColumn: "Id");
    }
}
