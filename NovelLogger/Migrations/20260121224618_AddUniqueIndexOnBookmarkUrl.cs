using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnBookmarkUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId_NovelId_DateAdded",
                table: "Bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_NovelId_Url",
                table: "Bookmarks",
                columns: new[] { "UserId", "NovelId", "Url" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId_NovelId_Url",
                table: "Bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_NovelId_DateAdded",
                table: "Bookmarks",
                columns: new[] { "UserId", "NovelId", "DateAdded" });
        }
    }
}
