using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionPlansAndAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanRemoveBranding",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRoleBasedAccess",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxAIRequests",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxContacts",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxFormSubmissions",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxPipelines",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CanRemoveBranding", "CreatedAt", "Description", "HasAnalytics", "HasRoleBasedAccess", "MaxAIRequests", "MaxContacts", "MaxFormSubmissions", "MaxForms", "MaxPipelines", "MaxPopupViews", "MaxPopups", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { false, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Perfect for testing and small projects", true, false, 0, 100, 50, 1, 1, 1000, 1, 0m, "Free", 0m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CanRemoveBranding", "CreatedAt", "Description", "HasLandingPageBuilder", "HasRoleBasedAccess", "MaxAIRequests", "MaxContacts", "MaxFormSubmissions", "MaxForms", "MaxLandingPages", "MaxPipelines", "MaxPopupViews", "MaxPopups", "MaxUsers", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { false, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Everything you need to start growing", true, false, 0, 1000, 500, 5, 3, 2, 10000, 3, 2, 29m, "Starter", "price_starter_monthly_29", "price_starter_yearly_288", "prod_starter_2026", 288m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CanRemoveBranding", "CreatedAt", "Description", "HasLandingPageBuilder", "HasPrioritySupport", "HasRoleBasedAccess", "HasWhiteLabel", "MaxAIRequests", "MaxContacts", "MaxFormSubmissions", "MaxForms", "MaxLandingPages", "MaxPipelines", "MaxPopupViews", "MaxPopups", "MaxUsers", "MaxWebsites", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { true, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), "AI-powered growth for serious businesses", true, false, true, true, 500, 25000, 5000, 25, 25, 10, 100000, 15, 10, 5, 79m, "Professional", "price_professional_monthly_79", "price_professional_yearly_804", "prod_professional_2026", 804m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CanRemoveBranding", "CreatedAt", "Description", "HasLandingPageBuilder", "HasRoleBasedAccess", "MaxAIRequests", "MaxContacts", "MaxFormSubmissions", "MaxForms", "MaxLandingPages", "MaxPipelines", "MaxWebsites", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { true, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Unlimited power for large organizations", true, true, -1, -1, -1, -1, -1, -1, -1, 199m, "Enterprise", "price_enterprise_monthly_199", "price_enterprise_yearly_2028", "prod_enterprise_2026", 2028m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanRemoveBranding",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "HasRoleBasedAccess",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxAIRequests",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxContacts",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxFormSubmissions",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxPipelines",
                table: "SubscriptionPlans");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "HasAnalytics", "MaxForms", "MaxPopupViews", "MaxPopups", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 342, DateTimeKind.Utc).AddTicks(9673), "Best for individuals just getting started", false, 0, 10000, 10, 7m, "Basic Plan", 70m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "HasLandingPageBuilder", "MaxForms", "MaxLandingPages", "MaxPopupViews", "MaxPopups", "MaxUsers", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 343, DateTimeKind.Utc).AddTicks(1881), "Best for growing businesses and creators", false, 0, 0, 50000, 25, 3, 17m, "Plus Plan", null, null, null, 170m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "HasLandingPageBuilder", "HasPrioritySupport", "HasWhiteLabel", "MaxForms", "MaxLandingPages", "MaxPopupViews", "MaxPopups", "MaxUsers", "MaxWebsites", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 343, DateTimeKind.Utc).AddTicks(1887), "Best for professionals and power users", false, true, false, 0, 0, -1, -1, 5, 1, 25m, "Pro Plan", null, null, null, 250m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "HasLandingPageBuilder", "MaxForms", "MaxLandingPages", "MaxWebsites", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 343, DateTimeKind.Utc).AddTicks(1890), "Best for teams and scaling businesses", false, 0, 0, 1, 37m, "Growth Plan", null, null, null, 370m });
        }
    }
}
