using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowerIdentifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flower_identifications",
                columns: table => new
                {
                    flower_identification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    embedding = table.Column<Vector>(type: "vector(768)", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flower_identifications", x => x.flower_identification_id);
                    table.ForeignKey(
                        name: "fk_flower_identifications_media_item_media_id",
                        column: x => x.media_id,
                        principalTable: "media_items",
                        principalColumn: "media_item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flower_predictions",
                columns: table => new
                {
                    flower_prediction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flower_identification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    scientific_name = table.Column<string>(type: "text", nullable: false),
                    common_name = table.Column<string>(type: "text", nullable: true),
                    family = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flower_predictions", x => x.flower_prediction_id);
                    table.ForeignKey(
                        name: "fk_flower_predictions_flower_identifications_flower_identifica",
                        column: x => x.flower_identification_id,
                        principalTable: "flower_identifications",
                        principalColumn: "flower_identification_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_flower_identifications_embedding",
                table: "flower_identifications",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_flower_identifications_media_id",
                table: "flower_identifications",
                column: "media_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_flower_predictions_flower_identification_id",
                table: "flower_predictions",
                column: "flower_identification_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flower_predictions");

            migrationBuilder.DropTable(
                name: "flower_identifications");
        }
    }
}
