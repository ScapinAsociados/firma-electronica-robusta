using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaElectronica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    IdEmpresa = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CUIT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Dominio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorPrincipal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.IdEmpresa);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    IdDocumento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdEmpresa = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSistemaOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdComprobanteOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TipoDocumento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    HashOriginal = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    HashFirmado = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RutaPdfOriginal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RutaPdfFirmado = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RutaConstancia = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    UltimoError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.IdDocumento);
                    table.ForeignKey(
                        name: "FK_Documentos_Empresas_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosApi",
                columns: table => new
                {
                    IdUsuarioApi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdEmpresa = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Permisos = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoUso = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosApi", x => x.IdUsuarioApi);
                    table.ForeignKey(
                        name: "FK_UsuariosApi_Empresas_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Firmantes",
                columns: table => new
                {
                    IdFirmante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDocumento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CUIT_DNI = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OrdenFirma = table.Column<int>(type: "int", nullable: false),
                    EstadoFirma = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaVista = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmantes", x => x.IdFirmante);
                    table.ForeignKey(
                        name: "FK_Firmantes_Documentos_IdDocumento",
                        column: x => x.IdDocumento,
                        principalTable: "Documentos",
                        principalColumn: "IdDocumento",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventosAuditoria",
                columns: table => new
                {
                    IdEvento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDocumento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFirmante = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaHoraUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IP = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HashDocumentoEnEvento = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DatosJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAuditoria", x => x.IdEvento);
                    table.ForeignKey(
                        name: "FK_EventosAuditoria_Documentos_IdDocumento",
                        column: x => x.IdDocumento,
                        principalTable: "Documentos",
                        principalColumn: "IdDocumento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventosAuditoria_Firmantes_IdFirmante",
                        column: x => x.IdFirmante,
                        principalTable: "Firmantes",
                        principalColumn: "IdFirmante",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesFirma",
                columns: table => new
                {
                    IdSolicitud = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDocumento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFirmante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesFirma", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_SolicitudesFirma_Documentos_IdDocumento",
                        column: x => x.IdDocumento,
                        principalTable: "Documentos",
                        principalColumn: "IdDocumento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesFirma_Firmantes_IdFirmante",
                        column: x => x.IdFirmante,
                        principalTable: "Firmantes",
                        principalColumn: "IdFirmante",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_IdEmpresa_IdSistemaOrigen_IdComprobanteOrigen",
                table: "Documentos",
                columns: new[] { "IdEmpresa", "IdSistemaOrigen", "IdComprobanteOrigen" });

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_CUIT",
                table: "Empresas",
                column: "CUIT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosAuditoria_IdDocumento_FechaHoraUTC",
                table: "EventosAuditoria",
                columns: new[] { "IdDocumento", "FechaHoraUTC" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosAuditoria_IdFirmante",
                table: "EventosAuditoria",
                column: "IdFirmante");

            migrationBuilder.CreateIndex(
                name: "IX_Firmantes_IdDocumento",
                table: "Firmantes",
                column: "IdDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesFirma_IdDocumento",
                table: "SolicitudesFirma",
                column: "IdDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesFirma_IdFirmante",
                table: "SolicitudesFirma",
                column: "IdFirmante");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesFirma_TokenHash",
                table: "SolicitudesFirma",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosApi_ApiKeyHash",
                table: "UsuariosApi",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosApi_IdEmpresa",
                table: "UsuariosApi",
                column: "IdEmpresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosAuditoria");

            migrationBuilder.DropTable(
                name: "SolicitudesFirma");

            migrationBuilder.DropTable(
                name: "UsuariosApi");

            migrationBuilder.DropTable(
                name: "Firmantes");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
