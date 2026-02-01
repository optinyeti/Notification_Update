using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadsAndViewsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PopupId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Company = table.Column<string>(type: "TEXT", nullable: true),
                    CustomFields = table.Column<string>(type: "TEXT", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Referrer = table.Column<string>(type: "TEXT", nullable: true),
                    UtmSource = table.Column<string>(type: "TEXT", nullable: true),
                    UtmMedium = table.Column<string>(type: "TEXT", nullable: true),
                    UtmCampaign = table.Column<string>(type: "TEXT", nullable: true),
                    UtmTerm = table.Column<string>(type: "TEXT", nullable: true),
                    UtmContent = table.Column<string>(type: "TEXT", nullable: true),
                    ConsentGiven = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConsentText = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    LastContactedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ViewType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Popups_PopupId",
                        column: x => x.PopupId,
                        principalTable: "Popups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leads_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PopupViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PopupId = table.Column<int>(type: "INTEGER", nullable: false),
                    ViewType = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Subtitle = table.Column<string>(type: "TEXT", nullable: true),
                    CallToAction = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ShowConditions = table.Column<string>(type: "TEXT", nullable: true),
                    NextViewId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PopupViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PopupViews_Popups_PopupId",
                        column: x => x.PopupId,
                        principalTable: "Popups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 14, 33, 10, 592, DateTimeKind.Utc).AddTicks(1910));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 14, 33, 10, 592, DateTimeKind.Utc).AddTicks(4681));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 14, 33, 10, 592, DateTimeKind.Utc).AddTicks(4712));

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PopupId",
                table: "Leads",
                column: "PopupId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_TenantId",
                table: "Leads",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PopupViews_PopupId",
                table: "PopupViews",
                column: "PopupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "PopupViews");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 21, 32, 861, DateTimeKind.Utc).AddTicks(3788));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 21, 32, 861, DateTimeKind.Utc).AddTicks(4793));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 21, 32, 861, DateTimeKind.Utc).AddTicks(4827));
        }
    }
}
