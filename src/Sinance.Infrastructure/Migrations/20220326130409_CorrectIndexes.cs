using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations;

public partial class CorrectIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ImportMapping");

        migrationBuilder.DropTable(
            name: "ImportBank");

        migrationBuilder.AlterColumn<string>(
            name: "ShortName",
            table: "Category",
            type: "varchar(4)",
            maxLength: 4,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(3)",
            oldMaxLength: 3,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<bool>(
            name: "IsStandard",
            table: "Category",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(ulong),
            oldType: "bit");

        migrationBuilder.AlterColumn<bool>(
            name: "IsRegular",
            table: "Category",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(ulong),
            oldType: "bit");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "BankAccount",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext")
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<bool>(
            name: "Disabled",
            table: "BankAccount",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(ulong),
            oldType: "bit");

        migrationBuilder.CreateIndex(
            name: "IX_Category_Name_UserId",
            table: "Category",
            columns: new[] { "Name", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Category_ShortName_UserId",
            table: "Category",
            columns: new[] { "ShortName", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BankAccount_Name_UserId",
            table: "BankAccount",
            columns: new[] { "Name", "UserId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Category_Name_UserId",
            table: "Category");

        migrationBuilder.DropIndex(
            name: "IX_Category_ShortName_UserId",
            table: "Category");

        migrationBuilder.DropIndex(
            name: "IX_BankAccount_Name_UserId",
            table: "BankAccount");

        migrationBuilder.AlterColumn<string>(
            name: "ShortName",
            table: "Category",
            type: "varchar(3)",
            maxLength: 3,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(4)",
            oldMaxLength: 4,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<ulong>(
            name: "IsStandard",
            table: "Category",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<ulong>(
            name: "IsRegular",
            table: "Category",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "BankAccount",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(255)")
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<ulong>(
            name: "Disabled",
            table: "BankAccount",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.CreateTable(
            name: "ImportBank",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Delimiter = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImportContainsHeader = table.Column<ulong>(type: "bit", nullable: false),
                IsStandard = table.Column<ulong>(type: "bit", nullable: false),
                Name = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportBank", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "ImportMapping",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ColumnIndex = table.Column<int>(type: "int", nullable: false),
                ColumnName = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ColumnTypeId = table.Column<int>(type: "int", nullable: false),
                FormatValue = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImportBankId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportMapping", x => x.Id);
                table.ForeignKey(
                    name: "FK_ImportMapping_ImportBank_ImportBankId",
                    column: x => x.ImportBankId,
                    principalTable: "ImportBank",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_ImportMapping_ImportBankId",
            table: "ImportMapping",
            column: "ImportBankId");
    }
}
