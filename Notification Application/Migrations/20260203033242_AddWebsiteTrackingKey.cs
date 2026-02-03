using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteTrackingKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiType",
                table: "ApiUsages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "ApiUsages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiUsages",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TokensUsed",
                table: "ApiUsages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingKey",
                table: "AllowedWebsites",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiType",
                table: "ApiUsages");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "ApiUsages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApiUsages");

            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "ApiUsages");

            migrationBuilder.DropColumn(
                name: "TrackingKey",
                table: "AllowedWebsites");
        }
    }
}
