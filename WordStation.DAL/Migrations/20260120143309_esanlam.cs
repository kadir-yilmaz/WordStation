using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordStation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class esanlam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    WordId = table.Column<int>(type: "int", nullable: false),
                    SynonymGroupId = table.Column<int>(type: "int", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SynonymWords");

            migrationBuilder.DropTable(
                name: "SynonymGroups");
        }
    }
}
