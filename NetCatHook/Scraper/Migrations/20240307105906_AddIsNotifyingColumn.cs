using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCatHook.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNotifyingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotifying",
                table: "TgBotChats",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotifying",
                table: "TgBotChats");
        }
    }
}
