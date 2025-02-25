using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NetCatHook.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class RenameTableSubjectChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "TgBotChats", newName: "SubjectChats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "SubjectChats", newName: "TgBotChats");
        }
    }
}
