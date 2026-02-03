using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationsAndPhoneTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_AspNetUsers_OwnerId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Tenants_TenantId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Popups_PopupId",
                table: "Leads");

            // Column already renamed manually, skip this operation
            // migrationBuilder.RenameColumn(
            //     name: "Type",
            //     table: "LeadActivities",
            //     newName: "ActivityType");

            // Integrations table doesn't exist yet, these renames are not needed
            // migrationBuilder.RenameColumn(
            //     name: "IsEnabled",
            //     table: "Integrations",
            //     newName: "SyncPopups");

            // migrationBuilder.RenameColumn(
            //     name: "Configuration",
            //     table: "Integrations",
            //     newName: "Settings");

            migrationBuilder.AlterColumn<int>(
                name: "PopupId",
                table: "Leads",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "AssignedDealId",
                table: "Leads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Disposition",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PotentialValue",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiSecret",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Integrations",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ErrorCount",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalAccountId",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalListId",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConnected",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastErrorAt",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthState",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SyncCount",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SyncEvents",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SyncForms",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SyncLeads",
                table: "Integrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiresAt",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookSecret",
                table: "Integrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IntegrationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IntegrationId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: true),
                    FormSubmissionId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationLogs_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntegrationLogs_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FriendlyName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TwilioSid = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    AreaCode = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ForwardToNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PurchasedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CanceledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    PerMinuteRate = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    UtmSource = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmMedium = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmCampaign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmTerm = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmContent = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TotalCalls = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LastCallDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneNumbers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FormType = table.Column<int>(type: "INTEGER", nullable: false),
                    Style = table.Column<int>(type: "INTEGER", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: true),
                    TextColor = table.Column<string>(type: "TEXT", nullable: true),
                    ButtonColor = table.Column<string>(type: "TEXT", nullable: true),
                    ButtonTextColor = table.Column<string>(type: "TEXT", nullable: true),
                    BorderColor = table.Column<string>(type: "TEXT", nullable: true),
                    BorderRadius = table.Column<int>(type: "INTEGER", nullable: false),
                    FontFamily = table.Column<string>(type: "TEXT", nullable: true),
                    FontSize = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomCss = table.Column<string>(type: "TEXT", nullable: true),
                    HeaderText = table.Column<string>(type: "TEXT", nullable: true),
                    SubheaderText = table.Column<string>(type: "TEXT", nullable: true),
                    SubmitButtonText = table.Column<string>(type: "TEXT", nullable: false),
                    FooterText = table.Column<string>(type: "TEXT", nullable: true),
                    ShowPoweredBy = table.Column<bool>(type: "INTEGER", nullable: false),
                    SubmissionAction = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RedirectUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SendNotificationEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationEmail = table.Column<string>(type: "TEXT", nullable: true),
                    CreateLead = table.Column<bool>(type: "INTEGER", nullable: false),
                    PipelineId = table.Column<int>(type: "INTEGER", nullable: true),
                    DefaultStageId = table.Column<int>(type: "INTEGER", nullable: true),
                    LeadSource = table.Column<string>(type: "TEXT", nullable: true),
                    EnableHoneypot = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableRecaptcha = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecaptchaSiteKey = table.Column<string>(type: "TEXT", nullable: true),
                    RecaptchaSecretKey = table.Column<string>(type: "TEXT", nullable: true),
                    RequireDoubleOptIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoubleOptInEmailTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    Views = table.Column<int>(type: "INTEGER", nullable: false),
                    Submissions = table.Column<int>(type: "INTEGER", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    EmbedCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebsiteForms_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebsiteForms_PipelineStages_DefaultStageId",
                        column: x => x.DefaultStageId,
                        principalTable: "PipelineStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebsiteForms_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebsiteForms_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneCalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumberId = table.Column<int>(type: "INTEGER", nullable: false),
                    CallSid = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FromNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ToNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ForwardedTo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CallDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    CallerCity = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CallerState = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CallerCountry = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CallerZip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    UtmSource = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmMedium = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmCampaign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmTerm = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UtmContent = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RecordingUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: true),
                    ConvertedToLead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneCalls_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhoneCalls_PhoneNumbers_PhoneNumberId",
                        column: x => x.PhoneNumberId,
                        principalTable: "PhoneNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Views = table.Column<int>(type: "INTEGER", nullable: false),
                    Submissions = table.Column<int>(type: "INTEGER", nullable: false),
                    UniqueViews = table.Column<int>(type: "INTEGER", nullable: false),
                    PartialSubmissions = table.Column<int>(type: "INTEGER", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    DesktopViews = table.Column<int>(type: "INTEGER", nullable: false),
                    MobileViews = table.Column<int>(type: "INTEGER", nullable: false),
                    TabletViews = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormAnalytics_WebsiteForms_FormId",
                        column: x => x.FormId,
                        principalTable: "WebsiteForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Placeholder = table.Column<string>(type: "TEXT", nullable: true),
                    HelpText = table.Column<string>(type: "TEXT", nullable: true),
                    FieldType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinLength = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxLength = table.Column<int>(type: "INTEGER", nullable: true),
                    ValidationPattern = table.Column<string>(type: "TEXT", nullable: true),
                    ValidationMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Options = table.Column<string>(type: "TEXT", nullable: true),
                    AllowOther = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasConditionalLogic = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConditionalLogic = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: true),
                    PreFillFromUrl = table.Column<bool>(type: "INTEGER", nullable: false),
                    LeadFieldMapping = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormFields_WebsiteForms_FormId",
                        column: x => x.FormId,
                        principalTable: "WebsiteForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    Referrer = table.Column<string>(type: "TEXT", nullable: true),
                    PageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    UtmSource = table.Column<string>(type: "TEXT", nullable: true),
                    UtmMedium = table.Column<string>(type: "TEXT", nullable: true),
                    UtmCampaign = table.Column<string>(type: "TEXT", nullable: true),
                    UtmContent = table.Column<string>(type: "TEXT", nullable: true),
                    UtmTerm = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSpam = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoubleOptInConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoubleOptInConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissions_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormSubmissions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormSubmissions_WebsiteForms_FormId",
                        column: x => x.FormId,
                        principalTable: "WebsiteForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Leads_AssignedDealId",
                table: "Leads",
                column: "AssignedDealId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_CreatedById",
                table: "Integrations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FormAnalytics_FormId",
                table: "FormAnalytics",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormId",
                table: "FormFields",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_FormId",
                table: "FormSubmissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_LeadId",
                table: "FormSubmissions",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_TenantId",
                table: "FormSubmissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationLogs_IntegrationId",
                table: "IntegrationLogs",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationLogs_LeadId",
                table: "IntegrationLogs",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneCalls_LeadId",
                table: "PhoneCalls",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneCalls_PhoneNumberId",
                table: "PhoneCalls",
                column: "PhoneNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumbers_UserId",
                table: "PhoneNumbers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteForms_CreatedById",
                table: "WebsiteForms",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteForms_DefaultStageId",
                table: "WebsiteForms",
                column: "DefaultStageId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteForms_PipelineId",
                table: "WebsiteForms",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteForms_TenantId",
                table: "WebsiteForms",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_AspNetUsers_OwnerId",
                table: "Deals",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Tenants_TenantId",
                table: "Deals",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Integrations_AspNetUsers_CreatedById",
                table: "Integrations",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Deals_AssignedDealId",
                table: "Leads",
                column: "AssignedDealId",
                principalTable: "Deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Popups_PopupId",
                table: "Leads",
                column: "PopupId",
                principalTable: "Popups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_AspNetUsers_OwnerId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Tenants_TenantId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Integrations_AspNetUsers_CreatedById",
                table: "Integrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Deals_AssignedDealId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Popups_PopupId",
                table: "Leads");

            migrationBuilder.DropTable(
                name: "FormAnalytics");

            migrationBuilder.DropTable(
                name: "FormFields");

            migrationBuilder.DropTable(
                name: "FormSubmissions");

            migrationBuilder.DropTable(
                name: "IntegrationLogs");

            migrationBuilder.DropTable(
                name: "PhoneCalls");

            migrationBuilder.DropTable(
                name: "WebsiteForms");

            migrationBuilder.DropTable(
                name: "PhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_Leads_AssignedDealId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Integrations_CreatedById",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "AssignedDealId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Disposition",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PotentialValue",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ApiSecret",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ErrorCount",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ExternalAccountId",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ExternalListId",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "IsConnected",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "LastErrorAt",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "OAuthState",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "SyncCount",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "SyncEvents",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "SyncForms",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "SyncLeads",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "TokenExpiresAt",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "WebhookSecret",
                table: "Integrations");

            // Column already renamed manually, skip reverse operation
            // migrationBuilder.RenameColumn(
            //     name: "ActivityType",
            //     table: "LeadActivities",
            //     newName: "Type");

            // Integrations table will be dropped, these renames are not needed
            // migrationBuilder.RenameColumn(
            //     name: "SyncPopups",
            //     table: "Integrations",
            //     newName: "IsEnabled");

            // migrationBuilder.RenameColumn(
            //     name: "Settings",
            //     table: "Integrations",
            //     newName: "Configuration");

            migrationBuilder.AlterColumn<int>(
                name: "PopupId",
                table: "Leads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_AspNetUsers_OwnerId",
                table: "Deals",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Tenants_TenantId",
                table: "Deals",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Popups_PopupId",
                table: "Leads",
                column: "PopupId",
                principalTable: "Popups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
