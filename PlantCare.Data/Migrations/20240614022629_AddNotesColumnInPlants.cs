using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesColumnInPlants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Plants",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Plants");
        }
    }
}
