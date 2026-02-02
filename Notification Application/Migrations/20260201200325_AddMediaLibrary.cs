using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaLibraryImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AltText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaLibraryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaLibraryImages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "MaxPopupViews", "MaxPopups", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 1, 20, 3, 24, 138, DateTimeKind.Utc).AddTicks(6630), "Best for individuals just getting started", 10000, 10, 7m, "Basic Plan", 70m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "HasAPIAccess", "MaxUsers", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 1, 20, 3, 24, 138, DateTimeKind.Utc).AddTicks(9435), "Best for growing businesses and creators", false, 3, 17m, "Plus Plan", 170m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "HasWhiteLabel", "MaxUsers", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 2, 1, 20, 3, 24, 138, DateTimeKind.Utc).AddTicks(9442), "Best for professionals and power users", false, 5, 25m, "Pro Plan", 250m });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "CreatedAt", "Description", "HasAPIAccess", "HasAdvancedTargeting", "HasAnalytics", "HasPrioritySupport", "HasWhiteLabel", "IsActive", "MaxPopupViews", "MaxPopups", "MaxUsers", "MonthlyPrice", "Name", "StripePriceIdMonthly", "StripePriceIdYearly", "StripeProductId", "YearlyPrice" },
                values: new object[] { 4, new DateTime(2026, 2, 1, 20, 3, 24, 138, DateTimeKind.Utc).AddTicks(9445), "Best for teams and scaling businesses", true, true, true, true, true, true, -1, -1, -1, 37m, "Growth Plan", null, null, null, 370m });

            migrationBuilder.CreateIndex(
                name: "IX_MediaLibraryImages_UserId",
                table: "MediaLibraryImages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaLibraryImages");

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "MaxPopupViews", "MaxPopups", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 1, 25, 14, 33, 10, 592, DateTimeKind.Utc).AddTicks(1910), "Perfect for getting started", 1000, 3, 0m, "Free", 0m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "HasAPIAccess", "MaxUsers", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 1, 25, 14, 33, 10, 592, DateTimeKind.Utc).AddTicks(4681), "For growing businesses", true, 5, 29m, "Professional", 290m });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "HasWhiteLabel", "MaxUsers", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { new DateTime(2026, 1, 25, 14, 33, 10, 592, DateTimeKind.Utc).AddTicks(4712), "For large organizations", true, -1, 99m, "Enterprise", 990m });
        }
    }
}
