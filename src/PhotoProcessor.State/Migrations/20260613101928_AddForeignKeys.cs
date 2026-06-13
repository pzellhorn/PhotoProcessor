using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_tags_tag_type_id",
                table: "tags",
                column: "tag_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_items_media_id",
                table: "tag_items",
                column: "media_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_items_tag_id",
                table: "tag_items",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_media_id",
                table: "jobs",
                column: "media_id");

            migrationBuilder.AddForeignKey(
                name: "fk_jobs_media_item_media_id",
                table: "jobs",
                column: "media_id",
                principalTable: "media_items",
                principalColumn: "media_item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tag_items_media_items_media_id",
                table: "tag_items",
                column: "media_id",
                principalTable: "media_items",
                principalColumn: "media_item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tag_items_tags_tag_id",
                table: "tag_items",
                column: "tag_id",
                principalTable: "tags",
                principalColumn: "tag_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_tag_type_tag_type_id",
                table: "tags",
                column: "tag_type_id",
                principalTable: "tag_types",
                principalColumn: "tag_type_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_jobs_media_item_media_id",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "fk_tag_items_media_items_media_id",
                table: "tag_items");

            migrationBuilder.DropForeignKey(
                name: "fk_tag_items_tags_tag_id",
                table: "tag_items");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_tag_type_tag_type_id",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tags_tag_type_id",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tag_items_media_id",
                table: "tag_items");

            migrationBuilder.DropIndex(
                name: "ix_tag_items_tag_id",
                table: "tag_items");

            migrationBuilder.DropIndex(
                name: "ix_jobs_media_id",
                table: "jobs");
        }
    }
}
