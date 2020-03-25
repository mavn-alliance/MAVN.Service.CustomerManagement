using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class AddCustomersRegistrationReferralInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers_registration_referral_data",
                schema: "customer_management");
        }
    }
}
