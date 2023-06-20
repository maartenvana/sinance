using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Storage.Migrations;

public partial class RemoveBudgets : Migration
{
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Budget",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Amount = table.Column<decimal>(nullable: true),
                CategoryId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Budget", x => x.Id);
                table.ForeignKey(
                    name: "FK_Budget_Category_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Category",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Budget_CategoryId",
            table: "Budget",
            column: "CategoryId");
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Budget");
    }
}