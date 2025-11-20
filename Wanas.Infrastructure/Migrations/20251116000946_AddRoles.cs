using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Wanas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "AspNetRoles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetRoles",
                type: "bit",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1bb1f291-7b9c-4071-9db8-7a8240138a44", "94016b54-918e-496a-a280-c20e922839a8", "ApplicationRole", false, false, "Admin", "ADMIN" },
                    { "81ae9adf-5636-4cbb-ac47-96eefcc348c0", "76ccf8b9-9885-458b-811a-4bdd707c235e", "ApplicationRole", true, false, "Renter", "RENTER" },
                    { "cc1e603c-66e6-4f71-bf93-cff53ed896d3", "c5df770b-786a-4cd0-aa0f-8b67eb126db9", "ApplicationRole", false, false, "Owner", "OWNER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1bb1f291-7b9c-4071-9db8-7a8240138a44", "808cfe62-dd5b-4c25-837d-3df87add03cb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "81ae9adf-5636-4cbb-ac47-96eefcc348c0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cc1e603c-66e6-4f71-bf93-cff53ed896d3");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1bb1f291-7b9c-4071-9db8-7a8240138a44", "808cfe62-dd5b-4c25-837d-3df87add03cb" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1bb1f291-7b9c-4071-9db8-7a8240138a44");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetRoles");
        }
    }
}
