using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddNovelStatusToNovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NovelStatus",
                table: "Novels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NovelStatus",
                table: "Novels");
        }
    }
}
