using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class RenameVerificationCodesTableAndAddNewOneForPhones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("verification_codes", "customer_management", "email_verification_codes", "customer_management");

            migrationBuilder.CreateTable(
                name: "phone_verification_codes",
                schema: "customer_management",
                columns: table => new
                {
                    customer_id = table.Column<string>(nullable: false),
                    code = table.Column<string>(nullable: false),
                    is_verified = table.Column<bool>(nullable: false),
                    expire_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phone_verification_codes", x => x.customer_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_phone_verification_codes_customer_id_code",
                schema: "customer_management",
                table: "phone_verification_codes",
                columns: new[] { "customer_id", "code" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("email_verification_codes", "customer_management", "verification_codes", "customer_management");

            migrationBuilder.DropTable(
                name: "phone_verification_codes",
                schema: "customer_management");
        }
    }
}
