using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogApp.Data.Migrations
{
    public partial class Renaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Blog_BlogID",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Blog");

            migrationBuilder.RenameColumn(
                name: "BlogID",
                table: "Comment",
                newName: "BlogId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_BlogID",
                table: "Comment",
                newName: "IX_Comment_BlogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blog_BlogId",
                table: "Comment",
                column: "BlogId",
                principalTable: "Blog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Blog_BlogId",
                table: "Comment");

            migrationBuilder.RenameColumn(
                name: "BlogId",
                table: "Comment",
                newName: "BlogID");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_BlogId",
                table: "Comment",
                newName: "IX_Comment_BlogID");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blog_BlogID",
                table: "Comment",
                column: "BlogID",
                principalTable: "Blog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
