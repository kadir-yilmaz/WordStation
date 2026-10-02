using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DailyWordRelationalRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedWordsJson",
                table: "DailyWordSessions");

            migrationBuilder.DropColumn(
                name: "DailyWordsJson",
                table: "DailyWordSessions");

            migrationBuilder.DropColumn(
                name: "RemainingWordIdsJson",
                table: "DailyWordSessions");

            migrationBuilder.CreateTable(
                name: "DailyWordSessionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWordSessionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWordSessionItems_DailyWordSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DailyWordSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyWordSessionItems_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyWordSessionItems_SessionId",
                table: "DailyWordSessionItems",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyWordSessionItems_WordId",
                table: "DailyWordSessionItems",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyWordSessionItems");

            migrationBuilder.AddColumn<string>(
                name: "CompletedWordsJson",
                table: "DailyWordSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DailyWordsJson",
                table: "DailyWordSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemainingWordIdsJson",
                table: "DailyWordSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
