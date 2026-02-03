using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddFormMultiStepFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowBackNavigation",
                table: "WebsiteForms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Effect",
                table: "WebsiteForms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowProgressBar",
                table: "WebsiteForms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowStepNumbers",
                table: "WebsiteForms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StepConfiguration",
                table: "WebsiteForms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StepType",
                table: "WebsiteForms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowBackNavigation",
                table: "WebsiteForms");

            migrationBuilder.DropColumn(
                name: "Effect",
                table: "WebsiteForms");

            migrationBuilder.DropColumn(
                name: "ShowProgressBar",
                table: "WebsiteForms");

            migrationBuilder.DropColumn(
                name: "ShowStepNumbers",
                table: "WebsiteForms");

            migrationBuilder.DropColumn(
                name: "StepConfiguration",
                table: "WebsiteForms");

            migrationBuilder.DropColumn(
                name: "StepType",
                table: "WebsiteForms");
        }
    }
}
