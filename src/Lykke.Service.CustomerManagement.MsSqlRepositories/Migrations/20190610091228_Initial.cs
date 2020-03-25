using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer_management");

            migrationBuilder.CreateTable(
                name: "verification_codes",
                schema: "customer_management",
                columns: table => new
                {
                    customer_id = table.Column<string>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    is_verified = table.Column<bool>(nullable: false),
                    expire_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_codes", x => x.customer_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_verification_codes_code",
                schema: "customer_management",
                table: "verification_codes",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "verification_codes",
                schema: "customer_management");
        }
    }
}
