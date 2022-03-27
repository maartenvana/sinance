using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinance.Storage.Migrations
{
    public partial class OptionalImportTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ImportTransactions_SourceTransactionId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "SourceTransactionId",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ImportTransactions_SourceTransactionId",
                table: "Transactions",
                column: "SourceTransactionId",
                principalTable: "ImportTransactions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ImportTransactions_SourceTransactionId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "SourceTransactionId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ImportTransactions_SourceTransactionId",
                table: "Transactions",
                column: "SourceTransactionId",
                principalTable: "ImportTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
