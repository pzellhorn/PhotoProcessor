using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoProcessor.State.Migrations
{
    /// <inheritdoc />
    public partial class AddJobError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "error",
                table: "jobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error",
                table: "jobs");
        }
    }
}
