using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "VerificationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "VerificationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "VerificationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "VerificationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataSchemaJson",
                table: "RequiredDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresDocumentNumber",
                table: "RequiredDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresExpiryDate",
                table: "RequiredDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresIssueDate",
                table: "RequiredDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "MetadataSchemaJson",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "RequiresDocumentNumber",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "RequiresExpiryDate",
                table: "RequiredDocuments");

            migrationBuilder.DropColumn(
                name: "RequiresIssueDate",
                table: "RequiredDocuments");
        }
    }
}
