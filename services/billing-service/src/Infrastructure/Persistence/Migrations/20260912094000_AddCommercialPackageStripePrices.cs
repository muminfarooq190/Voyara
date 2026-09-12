using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BillingService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BillingDbContext))]
[Migration("20260912094000_AddCommercialPackageStripePrices")]
public sealed class AddCommercialPackageStripePrices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "stripe_price_id_monthly", table: "commercial_packages",
            type: "character varying(80)", maxLength: 80, nullable: true);
        migrationBuilder.AddColumn<string>(name: "stripe_price_id_annual", table: "commercial_packages",
            type: "character varying(80)", maxLength: 80, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "stripe_price_id_monthly", table: "commercial_packages");
        migrationBuilder.DropColumn(name: "stripe_price_id_annual", table: "commercial_packages");
    }
}
