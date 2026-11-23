using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "health_events",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "financial_records",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "farm_activities",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "animals",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "health_events");

            migrationBuilder.DropColumn(
                name: "version",
                table: "financial_records");

            migrationBuilder.DropColumn(
                name: "version",
                table: "farm_activities");

            migrationBuilder.DropColumn(
                name: "version",
                table: "animals");
        }
    }
}
