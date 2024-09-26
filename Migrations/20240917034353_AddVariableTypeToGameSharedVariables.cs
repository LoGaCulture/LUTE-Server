using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LUTE_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddVariableTypeToGameSharedVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VariableType",
                table: "GameSharedVariables",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VariableType",
                table: "GameSharedVariables");
        }
    }
}
