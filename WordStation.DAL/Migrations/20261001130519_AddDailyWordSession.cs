using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyWordSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyWordSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ListName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DailyWordsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletedWordsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemainingWordIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWordSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyWordSessions_UserId_ListName",
                table: "DailyWordSessions",
                columns: new[] { "UserId", "ListName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyWordSessions");
        }
    }
}
