using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserDevicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "UserDocuments",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VerificationRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VerificationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxDevicesPerUser",
                table: "Tenants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Scopes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Scopes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RequiredDocuments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RequiredDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RequiredDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserDeviceId",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClientApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ClientApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleDeviceLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxDevices = table.Column<int>(type: "int", nullable: false),
                    ApplicationRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDeviceLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleDeviceLimits_AspNetRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleDeviceLimits_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FriendlyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserDeviceId",
                table: "RefreshTokens",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDeviceLimits_ApplicationRoleId",
                table: "RoleDeviceLimits",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDeviceLimits_RoleId",
                table: "RoleDeviceLimits",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId_DeviceFingerprint",
                table: "UserDevices",
                columns: new[] { "UserId", "DeviceFingerprint" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_UserDevices_UserDeviceId",
                table: "RefreshTokens",
                column: "UserDeviceId",
                principalTable: "UserDevices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_UserDevices_UserDeviceId",
                table: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RoleDeviceLimits");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserDeviceId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MaxDevicesPerUser",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "UserDeviceId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClientApplications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClientApplications");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "UserDocuments",
                newName: "UploadedAt");
        }
    }
}
