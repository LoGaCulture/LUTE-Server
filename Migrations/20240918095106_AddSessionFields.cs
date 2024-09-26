using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LUTE_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "Sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "Sessions");
        }
    }
}
