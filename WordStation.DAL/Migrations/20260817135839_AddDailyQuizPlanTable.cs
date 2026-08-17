using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQuizPlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyQuizPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ListName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DailyCount = table.Column<int>(type: "int", nullable: false),
                    ShuffledWordIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentPointer = table.Column<int>(type: "int", nullable: false),
                    LastCompletedDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StreakDays = table.Column<int>(type: "int", nullable: false),
                    IsEnglishToTurkish = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizPlans_UserId",
                table: "DailyQuizPlans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuizPlans");

            migrationBuilder.CreateTable(
                name: "SynonymGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynonymGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SynonymWords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SynonymGroupId = table.Column<int>(type: "int", nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynonymWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SynonymWords_SynonymGroups_SynonymGroupId",
                        column: x => x.SynonymGroupId,
                        principalTable: "SynonymGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SynonymWords_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SynonymWords_SynonymGroupId",
                table: "SynonymWords",
                column: "SynonymGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SynonymWords_WordId",
                table: "SynonymWords",
                column: "WordId");
        }
    }
}
