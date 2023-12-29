using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogApp.Data.Migrations
{
    public partial class add_author : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Blog");
        }
    }
}
