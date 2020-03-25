using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class AddRequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_verification_codes_code",
                schema: "customer_management",
                table: "verification_codes");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                schema: "customer_management",
                table: "verification_codes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_verification_codes_code",
                schema: "customer_management",
                table: "verification_codes",
                column: "code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_verification_codes_code",
                schema: "customer_management",
                table: "verification_codes");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                schema: "customer_management",
                table: "verification_codes",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_verification_codes_code",
                schema: "customer_management",
                table: "verification_codes",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");
        }
    }
}
