using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class AddStripeSubscriptionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The preceding migration already creates the billing/entitlement fields.
            // Add only the Stripe mapping required by the current model.
            migrationBuilder.CreateTable(
                name: "tenant_stripe_links",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_customer_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_tenant_stripe_links", x => x.tenant_id));
            migrationBuilder.CreateIndex(
                name: "IX_tenant_stripe_links_stripe_customer_id",
                table: "tenant_stripe_links", column: "stripe_customer_id", unique: true);
            migrationBuilder.AddColumn<string>(
                name: "stripe_subscription_id", table: "subscriptions",
                type: "character varying(80)", maxLength: 80, nullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_stripe_subscription_id",
                table: "subscriptions", column: "stripe_subscription_id", unique: true,
                filter: "stripe_subscription_id IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_subscriptions_stripe_subscription_id", table: "subscriptions");
            migrationBuilder.DropColumn(name: "stripe_subscription_id", table: "subscriptions");
            migrationBuilder.DropTable(name: "tenant_stripe_links");
        }
    }
}
