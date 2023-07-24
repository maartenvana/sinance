using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinance.Storage.Migrations
{
    public partial class ImportTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SourceTransactions_SourceTransactionId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SourceTransactions",
                table: "SourceTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SourceTransactions_CategoryId",
                table: "SourceTransactions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "SourceTransactions");

            migrationBuilder.RenameTable(
                name: "SourceTransactions",
                newName: "ImportTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_SourceTransactions_UserId",
                table: "ImportTransactions",
                newName: "IX_ImportTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SourceTransactions_BankAccountId",
                table: "ImportTransactions",
                newName: "IX_ImportTransactions_BankAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportTransactions",
                table: "ImportTransactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportTransactions_BankAccount_BankAccountId",
                table: "ImportTransactions",
                column: "BankAccountId",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportTransactions_Users_UserId",
                table: "ImportTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ImportTransactions_SourceTransactionId",
                table: "Transactions",
                column: "SourceTransactionId",
                principalTable: "ImportTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportTransactions_BankAccount_BankAccountId",
                table: "ImportTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportTransactions_Users_UserId",
                table: "ImportTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ImportTransactions_SourceTransactionId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportTransactions",
                table: "ImportTransactions");

            migrationBuilder.RenameTable(
                name: "ImportTransactions",
                newName: "SourceTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_ImportTransactions_UserId",
                table: "SourceTransactions",
                newName: "IX_SourceTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportTransactions_BankAccountId",
                table: "SourceTransactions",
                newName: "IX_SourceTransactions_BankAccountId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "SourceTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SourceTransactions",
                table: "SourceTransactions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SourceTransactions_CategoryId",
                table: "SourceTransactions",
                column: "CategoryId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SourceTransactions_SourceTransactionId",
                table: "Transactions",
                column: "SourceTransactionId",
                principalTable: "SourceTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
