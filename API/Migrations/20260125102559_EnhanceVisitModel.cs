using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceVisitModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "Visits",
                newName: "UserAgent");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Visits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Visits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Referrer",
                table: "Visits",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Referrer",
                table: "Visits");

            migrationBuilder.RenameColumn(
                name: "UserAgent",
                table: "Visits",
                newName: "Metadata");
        }
    }
}
