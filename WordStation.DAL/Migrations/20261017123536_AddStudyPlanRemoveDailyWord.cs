using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlanRemoveDailyWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyWordSessionItems");

            migrationBuilder.DropTable(
                name: "DailyWordSessions");

            migrationBuilder.CreateTable(
                name: "StudyPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DailyWordLimit = table.Column<int>(type: "int", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyPlanDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudyPlanId = table.Column<int>(type: "int", nullable: false),
                    DayNumber = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuizScore = table.Column<int>(type: "int", nullable: true),
                    QuizMaxScore = table.Column<int>(type: "int", nullable: true),
                    CorrectCount = table.Column<int>(type: "int", nullable: true),
                    WrongCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlanDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyPlanDays_StudyPlans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalTable: "StudyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyPlanLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudyPlanId = table.Column<int>(type: "int", nullable: false),
                    ListName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlanLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyPlanLists_StudyPlans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalTable: "StudyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyPlanDayWords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudyPlanDayId = table.Column<int>(type: "int", nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    IsCorrectInQuiz = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlanDayWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyPlanDayWords_StudyPlanDays_StudyPlanDayId",
                        column: x => x.StudyPlanDayId,
                        principalTable: "StudyPlanDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyPlanDayWords_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanDays_StudyPlanId",
                table: "StudyPlanDays",
                column: "StudyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanDayWords_StudyPlanDayId_WordId",
                table: "StudyPlanDayWords",
                columns: new[] { "StudyPlanDayId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanDayWords_WordId",
                table: "StudyPlanDayWords",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanLists_StudyPlanId",
                table: "StudyPlanLists",
                column: "StudyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_UserId",
                table: "StudyPlans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyPlanDayWords");

            migrationBuilder.DropTable(
                name: "StudyPlanLists");

            migrationBuilder.DropTable(
                name: "StudyPlanDays");

            migrationBuilder.DropTable(
                name: "StudyPlans");

            migrationBuilder.CreateTable(
                name: "DailyWordSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ListName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWordSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyWordSessionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_DailyWordSessions_UserId_ListName",
                table: "DailyWordSessions",
                columns: new[] { "UserId", "ListName" },
                unique: true);
        }
    }
}
