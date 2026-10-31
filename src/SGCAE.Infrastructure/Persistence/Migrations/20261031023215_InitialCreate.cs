using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCAE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identification = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstNames = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastNames = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credentials_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccessRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedCredentialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestedAccessPointCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccessPointId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessRecords_AccessPoints_AccessPointId",
                        column: x => x.AccessPointId,
                        principalTable: "AccessPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRecords_Credentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "Credentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessPoints_Code",
                table: "AccessPoints",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessRecords_AccessPointId_OccurredAtUtc",
                table: "AccessRecords",
                columns: new[] { "AccessPointId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessRecords_CredentialId",
                table: "AccessRecords",
                column: "CredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRecords_EmployeeId_AccessPointId_Result_OccurredAtUtc",
                table: "AccessRecords",
                columns: new[] { "EmployeeId", "AccessPointId", "Result", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessRecords_OccurredAtUtc",
                table: "AccessRecords",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_Code",
                table: "Credentials",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_EmployeeId_Status",
                table: "Credentials",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Identification",
                table: "Employees",
                column: "Identification",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Status",
                table: "Employees",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessRecords");

            migrationBuilder.DropTable(
                name: "AccessPoints");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
