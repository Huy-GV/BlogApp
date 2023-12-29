using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogApp.Data.Migrations
{
    public partial class User_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserID",
                table: "Comment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppUserID",
                table: "Blog",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AppUserID",
                table: "Comment",
                column: "AppUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_AppUserID",
                table: "Blog",
                column: "AppUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_AspNetUsers_AppUserID",
                table: "Blog",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_AppUserID",
                table: "Comment",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blog_AspNetUsers_AppUserID",
                table: "Blog");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AppUserID",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_AppUserID",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Blog_AppUserID",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "AppUserID",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "AppUserID",
                table: "Blog");
        }
    }
}
