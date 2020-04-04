using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Migrations
{
    public partial class CustomerFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_flags",
                schema: "customer_management");
        }
    }
}
