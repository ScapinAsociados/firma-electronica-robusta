using System.Text.Json;
using FirmaElectronica.Application.Documentos;
using FirmaElectronica.Application.Hashing;
using FirmaElectronica.Application.Security;
using FirmaElectronica.Application.Storage;
using FirmaElectronica.Domain.Common;
using FirmaElectronica.Domain.Entities;
using FirmaElectronica.Infrastructure.Options;
using FirmaElectronica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FirmaElectronica.Infrastructure.Documentos;

public sealed class DocumentosService(
    FirmaElectronicaDbContext dbContext,
    IFileStorage fileStorage,
    IPdfHashService pdfHashService,
    ISecureTokenService secureTokenService,
    IOptions<FirmaLinksOptions> firmaLinksOptions) : IDocumentosService
{
    public async Task<CrearDocumentoResponse> CrearAsync(
        CrearDocumentoRequest request,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var idDocumento = Guid.NewGuid();
        var idFirmante = Guid.NewGuid();
        var vencimiento = now.AddHours(firmaLinksOptions.Value.ExpirationHours);

        await using var buffer = new MemoryStream();
        await request.PdfStream.CopyToAsync(buffer, cancellationToken);

        buffer.Position = 0;
        var hashOriginal = await pdfHashService.ComputeSha256Async(buffer, cancellationToken);

        buffer.Position = 0;
        var storedFile = await fileStorage.SaveAsync(
            "pdf-originales",
            $"{idDocumento:N}.pdf",
            buffer,
            cancellationToken);

        var token = secureTokenService.GenerateToken();
        var documento = new Documento
        {
            IdDocumento = idDocumento,
            IdEmpresa = request.IdEmpresa,
            IdSistemaOrigen = request.IdSistemaOrigen,
            IdComprobanteOrigen = request.IdComprobanteOrigen,
            TipoDocumento = request.TipoDocumento.Trim(),
            NombreArchivoOriginal = request.NombreArchivo.Trim(),
            HashOriginal = hashOriginal,
            Estado = DocumentoEstados.PendienteFirma,
            FechaAlta = now,
            FechaVencimiento = vencimiento,
            RutaPdfOriginal = storedFile.Path,
            CreadoPor = request.CreadoPor
        };

        var firmante = new Firmante
        {
            IdFirmante = idFirmante,
            IdDocumento = idDocumento,
            Nombre = request.NombreFirmante.Trim(),
            CUIT_DNI = request.CuitDniFirmante,
            Email = request.EmailFirmante.Trim(),
            Celular = request.CelularFirmante,
            OrdenFirma = 1,
            EstadoFirma = "pendiente"
        };

        var solicitud = new SolicitudFirma
        {
            IdSolicitud = Guid.NewGuid(),
            IdDocumento = idDocumento,
            IdFirmante = idFirmante,
            TokenHash = token.TokenHash,
            FechaCreacion = now,
            FechaVencimiento = vencimiento,
            Estado = "pendiente"
        };

        dbContext.Documentos.Add(documento);
        dbContext.Firmantes.Add(firmante);
        dbContext.SolicitudesFirma.Add(solicitud);

        dbContext.EventosAuditoria.AddRange(
            CrearEvento(idDocumento, null, now, AuditoriaEventos.DocumentoCreado, "Documento creado.", auditoria, null, new
            {
                request.IdSistemaOrigen,
                request.IdComprobanteOrigen,
                request.TipoDocumento,
                request.NombreArchivo
            }),
            CrearEvento(idDocumento, idFirmante, now, AuditoriaEventos.PdfSubido, "PDF original almacenado.", auditoria, hashOriginal, new { storedFile.Path }),
            CrearEvento(idDocumento, idFirmante, now, AuditoriaEventos.HashCalculado, "Hash SHA-256 del PDF original calculado.", auditoria, hashOriginal, null),
            CrearEvento(idDocumento, idFirmante, now, AuditoriaEventos.LinkGenerado, "Link seguro de firma generado.", auditoria, hashOriginal, new { solicitud.FechaVencimiento }));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CrearDocumentoResponse
        {
            IdDocumento = idDocumento,
            Estado = documento.Estado,
            UrlFirma = BuildFirmaUrl(token.Token),
            HashOriginal = hashOriginal,
            FechaVencimiento = vencimiento
        };
    }

    public async Task<DocumentoEstadoResponse?> ObtenerEstadoAsync(
        Guid idDocumento,
        Guid idEmpresa,
        CancellationToken cancellationToken)
    {
        return await dbContext.Documentos
            .Where(x => x.IdDocumento == idDocumento && x.IdEmpresa == idEmpresa)
            .Select(x => new DocumentoEstadoResponse
            {
                IdDocumento = x.IdDocumento,
                Estado = x.Estado,
                FechaAlta = x.FechaAlta,
                FechaVencimiento = x.FechaVencimiento,
                FechaFirma = x.FechaFirma,
                UltimoError = x.UltimoError
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<FirmaImagenDocumentoResponse?> ObtenerFirmaImagenAsync(
        Guid idDocumento,
        Guid idEmpresa,
        CancellationToken cancellationToken)
    {
        var firmante = await dbContext.Firmantes
            .Where(x => x.IdDocumento == idDocumento &&
                x.Documento != null &&
                x.Documento.IdEmpresa == idEmpresa &&
                x.RutaFirmaImagen != null)
            .Select(x => new
            {
                x.IdFirmante,
                x.RutaFirmaImagen
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (firmante?.RutaFirmaImagen is null)
        {
            return null;
        }

        var stream = await fileStorage.OpenReadAsync(firmante.RutaFirmaImagen, cancellationToken);

        return new FirmaImagenDocumentoResponse
        {
            Content = stream,
            FileName = $"{idDocumento:N}-{firmante.IdFirmante:N}.png",
            ContentType = "image/png"
        };
    }

    private string BuildFirmaUrl(string token)
    {
        var baseUrl = firmaLinksOptions.Value.BaseUrl.TrimEnd('/');
        return $"{baseUrl}/f/{token}";
    }

    private static EventoAuditoria CrearEvento(
        Guid idDocumento,
        Guid? idFirmante,
        DateTime now,
        string tipoEvento,
        string descripcion,
        AuditoriaRequest auditoria,
        string? hashDocumento,
        object? datos)
    {
        return new EventoAuditoria
        {
            IdEvento = Guid.NewGuid(),
            IdDocumento = idDocumento,
            IdFirmante = idFirmante,
            FechaHoraUTC = now,
            TipoEvento = tipoEvento,
            Descripcion = descripcion,
            IP = auditoria.Ip,
            UserAgent = auditoria.UserAgent,
            HashDocumentoEnEvento = hashDocumento,
            DatosJson = datos is null ? null : JsonSerializer.Serialize(datos)
        };
    }
}
