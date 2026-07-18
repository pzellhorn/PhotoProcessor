using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoFrames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_media_id",
                table: "media_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "timestamp_ms",
                table: "media_items",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_items_parent_media_id",
                table: "media_items",
                column: "parent_media_id");

            migrationBuilder.AddForeignKey(
                name: "fk_media_items_media_items_parent_media_id",
                table: "media_items",
                column: "parent_media_id",
                principalTable: "media_items",
                principalColumn: "media_item_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_media_items_media_items_parent_media_id",
                table: "media_items");

            migrationBuilder.DropIndex(
                name: "ix_media_items_parent_media_id",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "parent_media_id",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "timestamp_ms",
                table: "media_items");
        }
    }
}
