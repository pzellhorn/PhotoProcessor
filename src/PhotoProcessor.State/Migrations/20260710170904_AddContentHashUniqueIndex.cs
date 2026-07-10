using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddContentHashUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_media_items_content_hash",
                table: "media_items",
                column: "content_hash",
                unique: true,
                filter: "content_hash <> '' AND is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_media_items_content_hash",
                table: "media_items");
        }
    }
}
