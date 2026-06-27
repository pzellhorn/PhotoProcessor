using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "fingerprints",
                columns: table => new
                {
                    fingerprint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    embedding = table.Column<Vector>(type: "vector(512)", nullable: false),
                    detection_score = table.Column<double>(type: "double precision", nullable: true),
                    bounding_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_y = table.Column<double>(type: "double precision", nullable: true),
                    bounding_width = table.Column<double>(type: "double precision", nullable: true),
                    bounding_height = table.Column<double>(type: "double precision", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fingerprints", x => x.fingerprint_id);
                    table.ForeignKey(
                        name: "fk_fingerprints_media_item_media_id",
                        column: x => x.media_id,
                        principalTable: "media_items",
                        principalColumn: "media_item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fingerprints_embedding",
                table: "fingerprints",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_fingerprints_media_id",
                table: "fingerprints",
                column: "media_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fingerprints");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
