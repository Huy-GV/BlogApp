using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleForum.Migrations
{
    /// <inheritdoc />
    public partial class RenameBlogToThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Comment_Blog_BlogId", table: "Comment");

            migrationBuilder.RenameTable(name: "Blog", newName: "Thread");
            migrationBuilder.DropPrimaryKey(name: "PK_Blog", table: "Thread");
            migrationBuilder.AddPrimaryKey(name: "PK_Thread", table: "Thread", column: "Id");
            migrationBuilder.DropForeignKey(name: "FK_Blog_AspNetUsers_AuthorUserName", table: "Thread");
            migrationBuilder.AddForeignKey(
                name: "FK_Thread_AspNetUsers_AuthorUserName", 
                table: "Thread", 
                column: "AuthorUserName", 
                principalTable: "AspNetUsers", 
                principalColumn: "UserName");

            migrationBuilder.RenameIndex(name: "IX_Blog_AuthorUserName", newName: "IX_Thread_AuthorUserName", table: "Thread");

            migrationBuilder.RenameColumn(name: "BlogId", newName: "ThreadId", table: "Comment");
            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Thread_ThreadId", 
                table: "Comment", 
                column: "ThreadId", 
                principalTable: "Thread", 
                principalColumn: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_BlogId", 
                newName: "IX_Comment_ThreadId", 
                table: "Comment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Thread", newName: "Blog");
            migrationBuilder.DropForeignKey(name: "FK_Comment_Thread_ThreadId", table: "Comment");

            migrationBuilder.RenameColumn(name: "ThreadId", newName: "BlogId", table: "Comment");
            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blog_BlogId",
                table: "Comment",
                column: "BlogId",
                principalTable: "Blog",
                principalColumn: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_ThreadId",
                newName: "IX_Comment_BlogId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Thread_AspNetUsers_AuthorUserName",
                table: "Blog");
            migrationBuilder.AddForeignKey(
                name: "FK_Blog_AspNetUsers_AuthorUserName",
                table: "Blog",
                column: "AuthorUserName",
                principalTable: "AspNetUsers",
                principalColumn: "UserName");

            migrationBuilder.DropPrimaryKey(name: "PK_Thread", table: "Blog");
            migrationBuilder.AddPrimaryKey(name: "PK_Blog", table: "Blog", column: "Id");

            migrationBuilder.RenameIndex(name: "IX_Thread_AuthorUserName", newName: "IX_Blog_AuthorUserName", table: "Blog");
        }
    }
}
