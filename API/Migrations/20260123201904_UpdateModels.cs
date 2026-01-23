using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Short",
                table: "Urls",
                newName: "ShortCode");

            migrationBuilder.RenameColumn(
                name: "Long",
                table: "Urls",
                newName: "OriginalUrl");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_Short",
                table: "Urls",
                newName: "IX_Urls_ShortCode");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Visits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ShortCode",
                table: "Urls",
                newName: "Short");

            migrationBuilder.RenameColumn(
                name: "OriginalUrl",
                table: "Urls",
                newName: "Long");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_ShortCode",
                table: "Urls",
                newName: "IX_Urls_Short");
        }
    }
}
