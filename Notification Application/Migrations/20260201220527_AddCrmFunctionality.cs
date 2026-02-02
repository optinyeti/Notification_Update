using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingCode",
                table: "Tenants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualRevenue",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AverageTimeOnSite",
                table: "Leads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailClicks",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmailOpens",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "Leads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstVisitAt",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScoreUpdate",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVisitAt",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadSource",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadSourceDetail",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageViews",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PipelineId",
                table: "Leads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StageEnteredAt",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "Leads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalVisits",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CrmTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "TEXT", nullable: true),
                    AssignedToId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmTasks_AspNetUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CrmTasks_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CrmTasks_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CrmTasks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    Probability = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpectedCloseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualCloseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deals_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deals_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    PerformedById = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    PageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PageTitle = table.Column<string>(type: "TEXT", nullable: true),
                    TimeOnPage = table.Column<int>(type: "INTEGER", nullable: true),
                    DeviceType = table.Column<string>(type: "TEXT", nullable: true),
                    Browser = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadActivities_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadActivities_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadCustomFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", nullable: false),
                    FieldLabel = table.Column<string>(type: "TEXT", nullable: false),
                    FieldType = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Options = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadCustomFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadCustomFields_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadScores_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadTags_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pipelines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pipelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pipelines_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadLeadTag",
                columns: table => new
                {
                    LeadsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadLeadTag", x => new { x.LeadsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_LeadLeadTag_LeadTags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "LeadTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadLeadTag_Leads_LeadsId",
                        column: x => x.LeadsId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PipelineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsWonStage = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLostStage = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpectedDaysInStage = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineStages_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadStageHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromStageId = table.Column<int>(type: "INTEGER", nullable: true),
                    ToStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MovedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    MovedById = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    DaysInPreviousStage = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadStageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadStageHistories_AspNetUsers_MovedById",
                        column: x => x.MovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadStageHistories_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadStageHistories_PipelineStages_FromStageId",
                        column: x => x.FromStageId,
                        principalTable: "PipelineStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadStageHistories_PipelineStages_ToStageId",
                        column: x => x.ToStageId,
                        principalTable: "PipelineStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 22, 5, 26, 779, DateTimeKind.Utc).AddTicks(266));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 22, 5, 26, 779, DateTimeKind.Utc).AddTicks(3791));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 22, 5, 26, 779, DateTimeKind.Utc).AddTicks(3799));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 22, 5, 26, 779, DateTimeKind.Utc).AddTicks(3802));

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerId",
                table: "Leads",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PipelineId",
                table: "Leads",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_StageId",
                table: "Leads",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_AssignedToId",
                table: "CrmTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_CreatedById",
                table: "CrmTasks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_LeadId",
                table: "CrmTasks",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_TenantId",
                table: "CrmTasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_LeadId",
                table: "Deals",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_OwnerId",
                table: "Deals",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_TenantId",
                table: "Deals",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_LeadId",
                table: "LeadActivities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_PerformedById",
                table: "LeadActivities",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_TenantId",
                table: "LeadActivities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadCustomFields_TenantId",
                table: "LeadCustomFields",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadLeadTag_TagsId",
                table: "LeadLeadTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadScores_LeadId",
                table: "LeadScores",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageHistories_FromStageId",
                table: "LeadStageHistories",
                column: "FromStageId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageHistories_LeadId",
                table: "LeadStageHistories",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageHistories_MovedById",
                table: "LeadStageHistories",
                column: "MovedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStageHistories_ToStageId",
                table: "LeadStageHistories",
                column: "ToStageId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadTags_TenantId",
                table: "LeadTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_TenantId",
                table: "Pipelines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineStages_PipelineId",
                table: "PipelineStages",
                column: "PipelineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_AspNetUsers_OwnerId",
                table: "Leads",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_PipelineStages_StageId",
                table: "Leads",
                column: "StageId",
                principalTable: "PipelineStages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Pipelines_PipelineId",
                table: "Leads",
                column: "PipelineId",
                principalTable: "Pipelines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_AspNetUsers_OwnerId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_PipelineStages_StageId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Pipelines_PipelineId",
                table: "Leads");

            migrationBuilder.DropTable(
                name: "CrmTasks");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropTable(
                name: "LeadActivities");

            migrationBuilder.DropTable(
                name: "LeadCustomFields");

            migrationBuilder.DropTable(
                name: "LeadLeadTag");

            migrationBuilder.DropTable(
                name: "LeadScores");

            migrationBuilder.DropTable(
                name: "LeadStageHistories");

            migrationBuilder.DropTable(
                name: "LeadTags");

            migrationBuilder.DropTable(
                name: "PipelineStages");

            migrationBuilder.DropTable(
                name: "Pipelines");

            migrationBuilder.DropIndex(
                name: "IX_Leads_OwnerId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_PipelineId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_StageId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TrackingCode",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "AnnualRevenue",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AverageTimeOnSite",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmailClicks",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmailOpens",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FirstVisitAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastScoreUpdate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastVisitAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadSource",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadSourceDetail",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PageViews",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PipelineId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "StageEnteredAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TotalVisits",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Leads");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 20, 43, 15, 874, DateTimeKind.Utc).AddTicks(1835));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 20, 43, 15, 874, DateTimeKind.Utc).AddTicks(6738));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 20, 43, 15, 874, DateTimeKind.Utc).AddTicks(6747));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 20, 43, 15, 874, DateTimeKind.Utc).AddTicks(6751));
        }
    }
}
