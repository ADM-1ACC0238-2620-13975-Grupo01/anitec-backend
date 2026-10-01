using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddCorralsAndAnimalCorral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "corral_id",
                table: "animals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "corrals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    herd_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_corrals", x => x.id);
                    table.ForeignKey(
                        name: "f_k_corrals__herd_herd_id",
                        column: x => x.herd_id,
                        principalTable: "herds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_animals_corral_id",
                table: "animals",
                column: "corral_id");

            migrationBuilder.CreateIndex(
                name: "i_x_corrals_herd_id",
                table: "corrals",
                column: "herd_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_animals__corral_corral_id",
                table: "animals",
                column: "corral_id",
                principalTable: "corrals",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_animals__corral_corral_id",
                table: "animals");

            migrationBuilder.DropTable(
                name: "corrals");

            migrationBuilder.DropIndex(
                name: "i_x_animals_corral_id",
                table: "animals");

            migrationBuilder.DropColumn(
                name: "corral_id",
                table: "animals");
        }
    }
}
