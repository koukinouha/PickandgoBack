using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApiProject.Migrations
{
    /// <inheritdoc />
    public partial class testtestttt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colis_AspNetUsers_UserId",
                table: "Colis");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Colis",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Colis_UserId",
                table: "Colis",
                newName: "IX_Colis_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colis_AspNetUsers_ApplicationUserId",
                table: "Colis",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colis_AspNetUsers_ApplicationUserId",
                table: "Colis");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Colis",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Colis_ApplicationUserId",
                table: "Colis",
                newName: "IX_Colis_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colis_AspNetUsers_UserId",
                table: "Colis",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
