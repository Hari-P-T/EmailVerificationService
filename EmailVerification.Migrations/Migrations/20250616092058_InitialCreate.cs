using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Integrate.EmailVerification.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StrictnessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrictnessTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailValidationResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    StrictnessTypeId = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ClientReferenceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailValidationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailValidationResults_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailValidationResults_StrictnessTypes_StrictnessTypeId",
                        column: x => x.StrictnessTypeId,
                        principalTable: "StrictnessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidationChecks",
                columns: table => new
                {
                    CheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Weightage = table.Column<int>(type: "integer", nullable: false),
                    StrictnessTypeId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationChecks", x => x.CheckId);
                    table.ForeignKey(
                        name: "FK_ValidationChecks_StrictnessTypes_StrictnessTypeId",
                        column: x => x.StrictnessTypeId,
                        principalTable: "StrictnessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailValidationCheckMappingsTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailValidationResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObtainedScore = table.Column<int>(type: "integer", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailValidationCheckMappingsTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailValidationCheckMappingsTable_EmailValidationResults_Em~",
                        column: x => x.EmailValidationResultId,
                        principalTable: "EmailValidationResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailValidationCheckMappingsTable_ValidationChecks_CheckId",
                        column: x => x.CheckId,
                        principalTable: "ValidationChecks",
                        principalColumn: "CheckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "StrictnessTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Basic" },
                    { 1, "Intermediate" },
                    { 2, "Advanced" }
                });

            migrationBuilder.InsertData(
                table: "ValidationChecks",
                columns: new[] { "CheckId", "CheckName", "CreatedAt", "CreatedBy", "Description", "IsActive", "IsDeleted", "StrictnessTypeId", "UpdatedAt", "UpdatedBy", "Weightage" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "UnRecognizedTLD", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Top-level domain is not recognized by ICANN.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "InvalidSyntax", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email address is syntactically invalid.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "InvalidDomainSpecificSyntax", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is invalid for the given domain.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "InvalidDNS", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Domain is unregistered or lacks A records.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "NoMXRecords", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Registered DNS does not have an MX record.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Established", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is in known bulk marketing lists.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Alias", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is believed to be an alias.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Bogus", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is likely a bogus.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "BogusSMSAddress", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is a bogus SMS domain address.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Garbage", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email contains garbage-like strokes or characters.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Vulgar", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email contains vulgar words or content.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "MailBoxIsFull", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Mailbox is full and cannot receive messages.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "MailboxIsBusy", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Mailbox is busy and cannot currently accept messages.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "DisposableEmail", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is believed to be a disposable address.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "KnownSpammer", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Email is known for spam-like activities.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "BlacklistedDomain", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Domain appears in one or more blacklists.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "KnownGreylister", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Domain server commonly uses greylisting techniques.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "OptInRequired", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Mail server opted in to send/receive emails.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "IsWhiteListOnly", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Given domain is whitelisted.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "ConnectionRefused", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Mail server refuses SMTP connection.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "EmailIsBad", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Critical failure in email verification like SPF/DMARC/DKIM.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 10 },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "IsNotACatchAll", new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), "Checks if the domain has a catch-all mailbox accepting all emails.", true, false, 1, new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001"), 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailValidationCheckMappingsTable_CheckId",
                table: "EmailValidationCheckMappingsTable",
                column: "CheckId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailValidationCheckMappingsTable_EmailValidationResultId",
                table: "EmailValidationCheckMappingsTable",
                column: "EmailValidationResultId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailValidationResults_Email",
                table: "EmailValidationResults",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmailValidationResults_RequestId",
                table: "EmailValidationResults",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailValidationResults_StrictnessTypeId",
                table: "EmailValidationResults",
                column: "StrictnessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationChecks_CheckName",
                table: "ValidationChecks",
                column: "CheckName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValidationChecks_StrictnessTypeId",
                table: "ValidationChecks",
                column: "StrictnessTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailValidationCheckMappingsTable");

            migrationBuilder.DropTable(
                name: "EmailValidationResults");

            migrationBuilder.DropTable(
                name: "ValidationChecks");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "StrictnessTypes");
        }
    }
}
