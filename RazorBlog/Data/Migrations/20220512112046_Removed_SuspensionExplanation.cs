using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogApp.Data.Migrations
{
    public partial class Removed_SuspensionExplanation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuspensionExplanation",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "SuspensionExplanation",
                table: "Blog");

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Comment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Blog",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Blog");

            migrationBuilder.AddColumn<string>(
                name: "SuspensionExplanation",
                table: "Comment",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionExplanation",
                table: "Blog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
