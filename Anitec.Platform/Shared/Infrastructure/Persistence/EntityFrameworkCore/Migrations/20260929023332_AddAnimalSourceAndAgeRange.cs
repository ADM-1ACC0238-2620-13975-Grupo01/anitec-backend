using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalSourceAndAgeRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "age_range",
                table: "animals",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "animals",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "age_range",
                table: "animals");

            migrationBuilder.DropColumn(
                name: "source",
                table: "animals");
        }
    }
}
