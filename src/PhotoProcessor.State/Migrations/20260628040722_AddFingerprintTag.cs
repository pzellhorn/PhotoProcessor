using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddFingerprintTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tag_id",
                table: "fingerprints",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fingerprints_tag_id",
                table: "fingerprints",
                column: "tag_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fingerprints_tag_tag_id",
                table: "fingerprints",
                column: "tag_id",
                principalTable: "tags",
                principalColumn: "tag_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fingerprints_tag_tag_id",
                table: "fingerprints");

            migrationBuilder.DropIndex(
                name: "ix_fingerprints_tag_id",
                table: "fingerprints");

            migrationBuilder.DropColumn(
                name: "tag_id",
                table: "fingerprints");
        }
    }
}
