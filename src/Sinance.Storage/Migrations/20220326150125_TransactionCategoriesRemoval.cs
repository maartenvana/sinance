using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class TransactionCategoriesRemoval : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TransactionCategory");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TransactionCategory",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                CategoryId = table.Column<int>(type: "int", nullable: false),
                TransactionId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TransactionCategory", x => x.Id);
                table.ForeignKey(
                    name: "FK_TransactionCategory_Category_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Category",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TransactionCategory_Transaction_TransactionId",
                    column: x => x.TransactionId,
                    principalTable: "Transaction",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_TransactionCategory_CategoryId",
            table: "TransactionCategory",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_TransactionCategory_TransactionId",
            table: "TransactionCategory",
            column: "TransactionId");
    }
}
