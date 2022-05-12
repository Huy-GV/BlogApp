using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogApp.Data.Migrations
{
    public partial class Renaming_and_Remodelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blog_AspNetUsers_AppUserID",
                table: "Blog");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AppUserID",
                table: "Comment");

            migrationBuilder.DropTable(
                name: "Suspension");

            migrationBuilder.DropColumn(
                name: "BanCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "AppUserID",
                table: "Comment",
                newName: "AppUserId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Comment",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_AppUserID",
                table: "Comment",
                newName: "IX_Comment_AppUserId");

            migrationBuilder.RenameColumn(
                name: "AppUserID",
                table: "Blog",
                newName: "AppUserId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Blog",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Blog",
                newName: "CoverImageUri");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Blog",
                newName: "Introduction");

            migrationBuilder.RenameIndex(
                name: "IX_Blog_AppUserID",
                table: "Blog",
                newName: "IX_Blog_AppUserId");

            migrationBuilder.RenameColumn(
                name: "ProfilePicture",
                table: "AspNetUsers",
                newName: "ProfileImageUri");

            migrationBuilder.AlterColumn<long>(
                name: "ViewCount",
                table: "Blog",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "BanTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Expiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanTicket_AspNetUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanTicket_UserName",
                table: "BanTicket",
                column: "UserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_AspNetUsers_AppUserId",
                table: "Blog",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_AppUserId",
                table: "Comment",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blog_AspNetUsers_AppUserId",
                table: "Blog");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AppUserId",
                table: "Comment");

            migrationBuilder.DropTable(
                name: "BanTicket");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Comment",
                newName: "AppUserID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Comment",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_AppUserId",
                table: "Comment",
                newName: "IX_Comment_AppUserID");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Blog",
                newName: "AppUserID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Blog",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Introduction",
                table: "Blog",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CoverImageUri",
                table: "Blog",
                newName: "ImagePath");

            migrationBuilder.RenameIndex(
                name: "IX_Blog_AppUserId",
                table: "Blog",
                newName: "IX_Blog_AppUserID");

            migrationBuilder.RenameColumn(
                name: "ProfileImageUri",
                table: "AspNetUsers",
                newName: "ProfilePicture");

            migrationBuilder.AlterColumn<int>(
                name: "ViewCount",
                table: "Blog",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "BanCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Suspension",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Expiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suspension", x => x.ID);
                });

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
    }
}
