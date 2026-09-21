using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiPlanAndOpenBuffetSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletedWordIdsJson",
                table: "DailyQuizPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DailySelectedWordIdsJson",
                table: "DailyQuizPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DailyQuizPlans",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanType",
                table: "DailyQuizPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "DailyQuizPlans",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedWordIdsJson",
                table: "DailyQuizPlans");

            migrationBuilder.DropColumn(
                name: "DailySelectedWordIdsJson",
                table: "DailyQuizPlans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DailyQuizPlans");

            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "DailyQuizPlans");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "DailyQuizPlans");
        }
    }
}
