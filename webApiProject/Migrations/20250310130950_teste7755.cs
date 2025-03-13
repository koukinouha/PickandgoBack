using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApiProject.Migrations
{
    /// <inheritdoc />
    public partial class teste7755 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomDestinataire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelephoneDestinataire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdresseDestinataire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomUtilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelephoneUtilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdresseUtilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeTVA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreColis = table.Column<int>(type: "int", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodeGouvernorat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Localite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factures_Colis_ColisId",
                        column: x => x.ColisId,
                        principalTable: "Colis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Factures_ColisId",
                table: "Factures",
                column: "ColisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Factures");
        }
    }
}
