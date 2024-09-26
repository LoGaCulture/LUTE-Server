using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LUTE_Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GameSharedVariables");

            migrationBuilder.AddColumn<string>(
                name: "UUID",
                table: "GameSharedVariables",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UUID",
                table: "GameSharedVariables");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "GameSharedVariables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
