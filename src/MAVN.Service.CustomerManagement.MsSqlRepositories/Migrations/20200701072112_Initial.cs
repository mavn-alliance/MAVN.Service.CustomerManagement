using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer_management");

            migrationBuilder.CreateTable(
                name: "customer_flags",
                schema: "customer_management",
                columns: table => new
                {
                    customer_id = table.Column<string>(nullable: false),
                    is_blocked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_flags", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "customers_registration_referral_data",
                schema: "customer_management",
                columns: table => new
                {
                    customer_id = table.Column<string>(nullable: false),
                    referral_code = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers_registration_referral_data", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "email_verification_codes",
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
                    table.PrimaryKey("PK_email_verification_codes", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "phone_verification_codes",
                schema: "customer_management",
                columns: table => new
                {
                    customer_id = table.Column<string>(nullable: false),
                    code = table.Column<string>(nullable: false),
                    expire_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phone_verification_codes", x => x.customer_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_codes_code",
                schema: "customer_management",
                table: "email_verification_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phone_verification_codes_customer_id_code",
                schema: "customer_management",
                table: "phone_verification_codes",
                columns: new[] { "customer_id", "code" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_flags",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "customers_registration_referral_data",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "email_verification_codes",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "phone_verification_codes",
                schema: "customer_management");
        }
    }
}
