using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sinance.Infrastructure.Migrations
{
    public partial class IdAutoIncrement : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.Sql("SET foreign_key_checks = 0;");

            SetIdToAutoIncrement(migrationBuilder, "BankAccount");
            SetIdToAutoIncrement(migrationBuilder, "Category");
            SetIdToAutoIncrement(migrationBuilder, "CategoryMapping");
            SetIdToAutoIncrement(migrationBuilder, "CustomReport");
            SetIdToAutoIncrement(migrationBuilder, "CustomReportCategory");
            SetIdToAutoIncrement(migrationBuilder, "ImportBank");
            SetIdToAutoIncrement(migrationBuilder, "ImportMapping");
            SetIdToAutoIncrement(migrationBuilder, "Transaction");
            SetIdToAutoIncrement(migrationBuilder, "TransactionCategory");

            migrationBuilder.Sql("SET foreign_key_checks = 1;");
        }

        private static void SetIdToAutoIncrement(MigrationBuilder migrationBuilder, string tableName)
        {
            var addAutoIncrementToIds = "ALTER TABLE {0} MODIFY COLUMN {1} int auto_increment";

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: tableName,
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql(string.Format(addAutoIncrementToIds, tableName, "Id"));
        }
    }
}