using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventosSernaJrAPI.Migrations
{
    /// <inheritdoc />
    public partial class StockProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InInventory",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "InvReal",
                table: "Products",
                newName: "InStock");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 22, 19, 9, 7, 943, DateTimeKind.Utc).AddTicks(1856), new DateTime(2024, 11, 22, 19, 9, 7, 943, DateTimeKind.Utc).AddTicks(2135) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InStock",
                table: "Products",
                newName: "InvReal");

            migrationBuilder.AddColumn<int>(
                name: "InInventory",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 22, 5, 50, 13, 902, DateTimeKind.Utc).AddTicks(3084), new DateTime(2024, 11, 22, 5, 50, 13, 902, DateTimeKind.Utc).AddTicks(3883) });
        }
    }
}
