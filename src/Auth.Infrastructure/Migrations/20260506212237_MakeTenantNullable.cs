using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeTenantNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequiredDocuments_Tenants_TenantId",
                table: "RequiredDocuments");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "VerificationRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "RequiredDocuments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RequiredDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TriggerRoleId",
                table: "RequiredDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDocuments_TriggerRoleId",
                table: "RequiredDocuments",
                column: "TriggerRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequiredDocuments_AspNetRoles_TriggerRoleId",
                table: "RequiredDocuments",
                column: "TriggerRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequiredDocuments_Tenants_TenantId",
                table: "RequiredDocuments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequiredDocuments_AspNetRoles_TriggerRoleId",
                table: "RequiredDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_RequiredDocuments_Tenants_TenantId",
                table: "RequiredDocuments");

            migrationBuilder.DropIndex(
                name: "IX_RequiredDocuments_TriggerRoleId",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "TriggerRoleId",
                table: "RequiredDocuments");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "VerificationRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "RequiredDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequiredDocuments_Tenants_TenantId",
                table: "RequiredDocuments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
