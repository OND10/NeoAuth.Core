using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScopeHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Scopes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_ParentId",
                table: "Scopes",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scopes_Scopes_ParentId",
                table: "Scopes",
                column: "ParentId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scopes_Scopes_ParentId",
                table: "Scopes");

            migrationBuilder.DropIndex(
                name: "IX_Scopes_ParentId",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Scopes");
        }
    }
}
