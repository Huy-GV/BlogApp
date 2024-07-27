using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleForum.Migrations
{
    /// <inheritdoc />
    public partial class AddReportTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Thread");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Comment");

            migrationBuilder.AddColumn<int>(
                name: "ReportTicketId",
                table: "Thread",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportTicketId",
                table: "Comment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThreadId = table.Column<int>(type: "int", nullable: true),
                    CommentId = table.Column<int>(type: "int", nullable: true),
                    ReportingUserName = table.Column<string>(type: "nvarchar(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTicket", x => x.Id);
                    table.CheckConstraint("PostId", "\r\n                ([ThreadId] IS NOT NULL AND [CommentId] IS NULL) OR\r\n                ([ThreadId] IS NULL AND [CommentId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ReportTicket_AspNetUsers_ReportingUserName",
                        column: x => x.ReportingUserName,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Thread_ReportTicketId",
                table: "Thread",
                column: "ReportTicketId",
                unique: true,
                filter: "[ReportTicketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ReportTicketId",
                table: "Comment",
                column: "ReportTicketId",
                unique: true,
                filter: "[ReportTicketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTicket_ReportingUserName",
                table: "ReportTicket",
                column: "ReportingUserName");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_ReportTicket_ReportTicketId",
                table: "Comment",
                column: "ReportTicketId",
                principalTable: "ReportTicket",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Thread_ReportTicket_ReportTicketId",
                table: "Thread",
                column: "ReportTicketId",
                principalTable: "ReportTicket",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_ReportTicket_ReportTicketId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Thread_ReportTicket_ReportTicketId",
                table: "Thread");

            migrationBuilder.DropTable(
                name: "ReportTicket");

            migrationBuilder.DropIndex(
                name: "IX_Thread_ReportTicketId",
                table: "Thread");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ReportTicketId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ReportTicketId",
                table: "Thread");

            migrationBuilder.DropColumn(
                name: "ReportTicketId",
                table: "Comment");

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Thread",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Comment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
