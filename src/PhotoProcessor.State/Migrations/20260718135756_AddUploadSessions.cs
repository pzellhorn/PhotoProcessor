using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "upload_sessions",
                columns: table => new
                {
                    upload_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    staging_key = table.Column<string>(type: "text", nullable: false),
                    multipart_upload_id = table.Column<string>(type: "text", nullable: false),
                    media_type = table.Column<int>(type: "integer", nullable: false),
                    total_bytes = table.Column<long>(type: "bigint", nullable: false),
                    received_bytes = table.Column<long>(type: "bigint", nullable: false),
                    next_part_number = table.Column<int>(type: "integer", nullable: false),
                    completed = table.Column<bool>(type: "boolean", nullable: false),
                    duplicate = table.Column<bool>(type: "boolean", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_upload_sessions", x => x.upload_session_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "upload_sessions");
        }
    }
}
