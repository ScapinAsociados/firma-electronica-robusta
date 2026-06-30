using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaElectronica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaImagenFirmante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RutaFirmaImagen",
                table: "Firmantes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RutaFirmaImagen",
                table: "Firmantes");
        }
    }
}
