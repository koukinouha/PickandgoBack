using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApiProject.Migrations
{
    /// <inheritdoc />
    public partial class tetetdqsdqsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fraisLivraison",
                table: "Colis",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fraisLivraison",
                table: "Colis");
        }
    }
}
