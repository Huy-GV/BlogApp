using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogApp.Data.Migrations
{
    public partial class PostUpdateTime_PostSoftDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Blog");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Comment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Comment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ToBeDeleted",
                table: "Comment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Blog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Blog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ToBeDeleted",
                table: "Blog",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ToBeDeleted",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "ToBeDeleted",
                table: "Blog");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Comment",
                type: "datetime2",
                maxLength: 255,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Blog",
                type: "datetime2",
                maxLength: 255,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
