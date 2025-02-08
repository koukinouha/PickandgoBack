using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApiProject.Migrations
{
    /// <inheritdoc />
    public partial class teste3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    role = table.Column<int>(type: "int", nullable: true),
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Colis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomComplet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeGouvernorat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delegation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Localite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArticles = table.Column<int>(type: "int", nullable: false),
                    Prix = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Echange = table.Column<bool>(type: "bit", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatutLivraison = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Annulation = table.Column<bool>(type: "bit", nullable: false),
                    DateEnlevement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateAjoutColis = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRetourColis = table.Column<DateTime>(type: "datetime2", nullable: false),
                    remboursment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colis_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colis_UserId",
                table: "Colis",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ProfileId",
                table: "User",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colis");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
