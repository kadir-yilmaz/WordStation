using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDailyQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPlanDayHistories");

            migrationBuilder.DropTable(
                name: "DailyQuizPlans");

            migrationBuilder.DropColumn(
                name: "IsDailyQuiz",
                table: "QuizHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDailyQuiz",
                table: "QuizHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DailyQuizPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPointer = table.Column<int>(type: "int", nullable: false),
                    DailyCount = table.Column<int>(type: "int", nullable: false),
                    IsEnglishToTurkish = table.Column<bool>(type: "bit", nullable: false),
                    LastCompletedDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ListName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShuffledWordIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreakDays = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyPlanDayHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyQuizPlanId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    DayNumber = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    ResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WrongCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPlanDayHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPlanDayHistories_DailyQuizPlans_DailyQuizPlanId",
                        column: x => x.DailyQuizPlanId,
                        principalTable: "DailyQuizPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlanDayHistories_DailyQuizPlanId",
                table: "DailyPlanDayHistories",
                column: "DailyQuizPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlanDayHistories_UserId",
                table: "DailyPlanDayHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizPlans_UserId",
                table: "DailyQuizPlans",
                column: "UserId");
        }
    }
}
