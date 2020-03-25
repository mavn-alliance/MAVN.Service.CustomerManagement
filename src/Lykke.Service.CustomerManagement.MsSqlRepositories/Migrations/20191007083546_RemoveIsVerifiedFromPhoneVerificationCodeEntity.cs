using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class RemoveIsVerifiedFromPhoneVerificationCodeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_verified",
                schema: "customer_management",
                table: "phone_verification_codes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                schema: "customer_management",
                table: "phone_verification_codes",
                nullable: false,
                defaultValue: false);
        }
    }
}
