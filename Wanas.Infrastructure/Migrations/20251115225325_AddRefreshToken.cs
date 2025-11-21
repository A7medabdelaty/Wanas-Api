using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Age", "Bio", "City", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FullName", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Photo", "ProfileType", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "808cfe62-dd5b-4c25-837d-3df87add03cb", 0, 30, "System Administrator", "Cairo", "d7b60902-76de-41cd-b394-4bcb7ad058e3", new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), "admin@wanas.com", true, "Wanas Admin", false, false, null, "ADMIN@WANAS.COM", "ADMIN@WANAS.COM", "AQAAAAIAAYagAAAAELa2X0xFqZjvpTNRlgpsUPiLoNS8BFTI6VQSb4aUwPk2Wk3UMJ90QYH832zEDknnkA==", "01234567890", false, "", "Admin", "71ac7750-1375-4d94-9b71-cfd70509373f", false, "admin@wanas.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "808cfe62-dd5b-4c25-837d-3df87add03cb");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "Messages");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
