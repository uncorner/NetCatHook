using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCatHook.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnIsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "TgBotChats",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "TgBotChats");
        }
    }
}
