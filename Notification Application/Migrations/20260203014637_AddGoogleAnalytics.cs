using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleAnalytics4Id",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleTagManagerId",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasLandingPageBuilder",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxForms",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxLandingPages",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxWebsites",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AllowedWebsites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedWebsites_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LandingPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    CustomDomain = table.Column<string>(type: "TEXT", nullable: true),
                    HtmlContent = table.Column<string>(type: "TEXT", nullable: false),
                    CssContent = table.Column<string>(type: "TEXT", nullable: true),
                    BuilderConfig = table.Column<string>(type: "TEXT", nullable: true),
                    MetaTitle = table.Column<string>(type: "TEXT", nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", nullable: true),
                    OgImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ConversionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPages_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LandingPages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    HasAddedWebsite = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasCreatedPopup = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasInstalledPixel = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasConnectedIntegration = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasConfiguredTargeting = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasViewedAnalytics = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasCustomizedBranding = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasDismissed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnboardingProgress_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebsitePages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MetaDescription = table.Column<string>(type: "TEXT", nullable: true),
                    MetaKeywords = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHomepage = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsitePages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebsitePages_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebsitePages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HasLandingPageBuilder", "MaxForms", "MaxLandingPages", "MaxWebsites" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 342, DateTimeKind.Utc).AddTicks(9673), false, 0, 0, 1 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HasLandingPageBuilder", "MaxForms", "MaxLandingPages", "MaxWebsites" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 343, DateTimeKind.Utc).AddTicks(1881), false, 0, 0, 1 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "HasLandingPageBuilder", "MaxForms", "MaxLandingPages", "MaxWebsites" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 343, DateTimeKind.Utc).AddTicks(1887), false, 0, 0, 1 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "HasLandingPageBuilder", "MaxForms", "MaxLandingPages", "MaxWebsites" },
                values: new object[] { new DateTime(2026, 2, 3, 1, 46, 36, 343, DateTimeKind.Utc).AddTicks(1890), false, 0, 0, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedWebsites_TenantId",
                table: "AllowedWebsites",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPages_CreatedById",
                table: "LandingPages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPages_TenantId",
                table: "LandingPages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingProgress_TenantId",
                table: "OnboardingProgress",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingProgress_UserId",
                table: "OnboardingProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsitePages_CreatedById",
                table: "WebsitePages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WebsitePages_TenantId",
                table: "WebsitePages",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedWebsites");

            migrationBuilder.DropTable(
                name: "LandingPages");

            migrationBuilder.DropTable(
                name: "OnboardingProgress");

            migrationBuilder.DropTable(
                name: "WebsitePages");

            migrationBuilder.DropColumn(
                name: "GoogleAnalytics4Id",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "GoogleTagManagerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "HasLandingPageBuilder",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxForms",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxLandingPages",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxWebsites",
                table: "SubscriptionPlans");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 2, 21, 47, 58, 574, DateTimeKind.Utc).AddTicks(4874));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 2, 21, 47, 58, 575, DateTimeKind.Utc).AddTicks(340));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 2, 21, 47, 58, 575, DateTimeKind.Utc).AddTicks(348));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 2, 21, 47, 58, 575, DateTimeKind.Utc).AddTicks(352));
        }
    }
}
