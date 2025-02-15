using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApiProject.Migrations
{
    /// <inheritdoc />
    public partial class test47 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateEnlevement",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "DateRetourColis",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "remboursment",
                table: "Colis");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateEnlevement",
                table: "Colis",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRetourColis",
                table: "Colis",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "remboursment",
                table: "Colis",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
