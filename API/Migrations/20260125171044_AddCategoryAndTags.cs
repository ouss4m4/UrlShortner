using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Urls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Urls",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Urls");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Urls");
        }
    }
}
