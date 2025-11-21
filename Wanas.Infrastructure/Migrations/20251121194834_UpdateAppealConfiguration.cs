using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppealConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_AspNetUsers_ReviewedByAdminId",
                table: "Appeals");

            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_AspNetUsers_UserId",
                table: "Appeals");

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_CreatedAt",
                table: "Appeals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_Status",
                table: "Appeals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_UserId_Status",
                table: "Appeals",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_AspNetUsers_ReviewedByAdminId",
                table: "Appeals",
                column: "ReviewedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_AspNetUsers_UserId",
                table: "Appeals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_AspNetUsers_ReviewedByAdminId",
                table: "Appeals");

            migrationBuilder.DropForeignKey(
                name: "FK_Appeals_AspNetUsers_UserId",
                table: "Appeals");

            migrationBuilder.DropIndex(
                name: "IX_Appeals_CreatedAt",
                table: "Appeals");

            migrationBuilder.DropIndex(
                name: "IX_Appeals_Status",
                table: "Appeals");

            migrationBuilder.DropIndex(
                name: "IX_Appeals_UserId_Status",
                table: "Appeals");

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_AspNetUsers_ReviewedByAdminId",
                table: "Appeals",
                column: "ReviewedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appeals_AspNetUsers_UserId",
                table: "Appeals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
