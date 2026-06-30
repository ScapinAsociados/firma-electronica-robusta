using System.Text.Json;
using FirmaElectronica.Application.Documentos;
using FirmaElectronica.Application.Firmas;
using FirmaElectronica.Application.Security;
using FirmaElectronica.Application.Storage;
using FirmaElectronica.Domain.Common;
using FirmaElectronica.Domain.Entities;
using FirmaElectronica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirmaElectronica.Infrastructure.Firmas;

public sealed class FirmaService(
    FirmaElectronicaDbContext dbContext,
    ISecureTokenService secureTokenService,
    IFileStorage fileStorage) : IFirmaService
{
    public async Task<TokenFirmaResponse> ValidarTokenAsync(
        string token,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken)
    {
        var solicitud = await BuscarSolicitudAsync(token, cancellationToken);
        var status = await EvaluarSolicitudAsync(solicitud, auditoria, cancellationToken);

        if (status != TokenFirmaStatus.Valido || solicitud is null)
        {
            return new TokenFirmaResponse
            {
                Status = status,
                IdDocumento = solicitud?.IdDocumento,
                EstadoDocumento = solicitud?.Documento?.Estado,
                FechaVencimiento = solicitud?.FechaVencimiento
            };
        }

        var now = DateTime.UtcNow;
        dbContext.EventosAuditoria.Add(CrearEvento(
            solicitud.IdDocumento,
            solicitud.IdFirmante,
            now,
            AuditoriaEventos.LinkAbierto,
            "Link de firma abierto.",
            auditoria,
            solicitud.Documento?.HashOriginal,
            null));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TokenFirmaResponse
        {
            Status = TokenFirmaStatus.Valido,
            IdDocumento = solicitud.IdDocumento,
            EstadoDocumento = solicitud.Documento?.Estado,
            FechaVencimiento = solicitud.FechaVencimiento
        };
    }

    public async Task<FirmaOperacionResult<DocumentoFirmaResponse>> ObtenerDocumentoAsync(
        string token,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken)
    {
        var solicitud = await BuscarSolicitudAsync(token, cancellationToken);
        var status = await EvaluarSolicitudAsync(solicitud, auditoria, cancellationToken);
        if (status != TokenFirmaStatus.Valido || solicitud?.Documento is null || solicitud.Firmante is null)
        {
            return FirmaOperacionResult<DocumentoFirmaResponse>.Failure(status, ObtenerMensajeStatus(status));
        }

        return FirmaOperacionResult<DocumentoFirmaResponse>.Success(new DocumentoFirmaResponse
        {
            IdDocumento = solicitud.Documento.IdDocumento,
            Estado = solicitud.Documento.Estado,
            TipoDocumento = solicitud.Documento.TipoDocumento,
            NombreArchivoOriginal = solicitud.Documento.NombreArchivoOriginal,
            HashOriginal = solicitud.Documento.HashOriginal,
            FechaVencimiento = solicitud.Documento.FechaVencimiento,
            Firmante = new FirmanteFirmaResponse
            {
                Nombre = solicitud.Firmante.Nombre,
                CuitDni = solicitud.Firmante.CUIT_DNI,
                Email = solicitud.Firmante.Email,
                Celular = solicitud.Firmante.Celular,
                EstadoFirma = solicitud.Firmante.EstadoFirma,
                TieneFirmaImagen = !string.IsNullOrWhiteSpace(solicitud.Firmante.RutaFirmaImagen)
            }
        });
    }

    public async Task<FirmaOperacionResult<PdfOriginalResponse>> ObtenerPdfOriginalAsync(
        string token,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken)
    {
        var solicitud = await BuscarSolicitudAsync(token, cancellationToken);
        var status = await EvaluarSolicitudAsync(solicitud, auditoria, cancellationToken);
        if (status != TokenFirmaStatus.Valido || solicitud?.Documento is null)
        {
            return FirmaOperacionResult<PdfOriginalResponse>.Failure(status, ObtenerMensajeStatus(status));
        }

        var now = DateTime.UtcNow;
        if (solicitud.Firmante is not null && solicitud.Firmante.FechaVista is null)
        {
            solicitud.Firmante.FechaVista = now;
        }

        if (solicitud.Documento.Estado == DocumentoEstados.PendienteFirma)
        {
            solicitud.Documento.Estado = DocumentoEstados.Visto;
        }

        var stream = await fileStorage.OpenReadAsync(solicitud.Documento.RutaPdfOriginal, cancellationToken);

        dbContext.EventosAuditoria.Add(CrearEvento(
            solicitud.IdDocumento,
            solicitud.IdFirmante,
            now,
            AuditoriaEventos.PdfVisualizado,
            "Documento PDF original entregado al firmante.",
            auditoria,
            solicitud.Documento.HashOriginal,
            null));

        await dbContext.SaveChangesAsync(cancellationToken);

        return FirmaOperacionResult<PdfOriginalResponse>.Success(new PdfOriginalResponse
        {
            Content = stream,
            FileName = solicitud.Documento.NombreArchivoOriginal,
            ContentType = "application/pdf"
        });
    }

    public async Task<FirmaOperacionResult<FirmaAceptadaResponse>> AceptarAsync(
        string token,
        AceptarFirmaRequest request,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken)
    {
        if (!request.AceptaTerminos || string.IsNullOrWhiteSpace(request.TextoAceptado))
        {
            return FirmaOperacionResult<FirmaAceptadaResponse>.Failure(
                TokenFirmaStatus.DocumentoNoDisponible,
                "Debe aceptar expresamente los terminos de firma electronica.");
        }

        if (string.IsNullOrWhiteSpace(request.FirmaImagenBase64) || request.FirmaPuntosCapturados <= 0)
        {
            return FirmaOperacionResult<FirmaAceptadaResponse>.Failure(
                TokenFirmaStatus.DocumentoNoDisponible,
                "Debe dibujar una firma o capturar una huella antes de confirmar.");
        }

        var solicitud = await BuscarSolicitudAsync(token, cancellationToken);
        var status = await EvaluarSolicitudAsync(solicitud, auditoria, cancellationToken);
        if (status != TokenFirmaStatus.Valido || solicitud?.Documento is null || solicitud.Firmante is null)
        {
            return FirmaOperacionResult<FirmaAceptadaResponse>.Failure(status, ObtenerMensajeStatus(status));
        }

        var now = DateTime.UtcNow;
        string? rutaFirmaImagen;
        try
        {
            rutaFirmaImagen = await GuardarFirmaImagenAsync(
                request.FirmaImagenBase64,
                solicitud.IdDocumento,
                solicitud.IdFirmante,
                cancellationToken);
        }
        catch (FormatException)
        {
            return FirmaOperacionResult<FirmaAceptadaResponse>.Failure(
                TokenFirmaStatus.DocumentoNoDisponible,
                "La imagen de firma no tiene un formato Base64 valido.");
        }

        if (rutaFirmaImagen is null)
        {
            return FirmaOperacionResult<FirmaAceptadaResponse>.Failure(
                TokenFirmaStatus.DocumentoNoDisponible,
                "Debe dibujar una firma o capturar una huella antes de confirmar.");
        }

        solicitud.Documento.Estado = DocumentoEstados.Firmado;
        solicitud.Documento.FechaFirma = now;
        solicitud.Firmante.EstadoFirma = "firmado";
        solicitud.Firmante.FechaFirma = now;
        solicitud.Firmante.RutaFirmaImagen = rutaFirmaImagen;
        solicitud.SolicitudFirmaFechaUsoOrNow(now);
        solicitud.Estado = "utilizado";

        dbContext.EventosAuditoria.AddRange(
            CrearEvento(
                solicitud.IdDocumento,
                solicitud.IdFirmante,
                now,
                AuditoriaEventos.TerminosAceptados,
                "Terminos de firma electronica aceptados.",
                auditoria,
                solicitud.Documento.HashOriginal,
                new
                {
                    request.TextoAceptado,
                    request.NombreConfirmado,
                    request.CuitDniConfirmado,
                    MetodoFirma = NormalizarMetodoFirma(request.FirmaMetodo),
                    TieneFirmaImagen = rutaFirmaImagen is not null,
                    request.FirmaPuntosCapturados
                }),
            CrearEvento(
                solicitud.IdDocumento,
                solicitud.IdFirmante,
                now,
                AuditoriaEventos.FirmaConfirmada,
                "Firma electronica confirmada por aceptacion.",
                auditoria,
                solicitud.Documento.HashOriginal,
                new
                {
                    RutaFirmaImagen = rutaFirmaImagen,
                    MetodoFirma = NormalizarMetodoFirma(request.FirmaMetodo),
                    request.FirmaPuntosCapturados
                }));

        await dbContext.SaveChangesAsync(cancellationToken);

        return FirmaOperacionResult<FirmaAceptadaResponse>.Success(new FirmaAceptadaResponse
        {
            IdDocumento = solicitud.IdDocumento,
            Estado = solicitud.Documento.Estado,
            FechaFirma = now
        });
    }

    private async Task<SolicitudFirma?> BuscarSolicitudAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHash = secureTokenService.HashToken(token);
        return await dbContext.SolicitudesFirma
            .Include(x => x.Documento)
            .Include(x => x.Firmante)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    private async Task<string?> GuardarFirmaImagenAsync(
        string? firmaImagenBase64,
        Guid idDocumento,
        Guid idFirmante,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(firmaImagenBase64))
        {
            return null;
        }

        const string dataUrlPrefix = "data:image/png;base64,";
        var base64 = firmaImagenBase64.StartsWith(dataUrlPrefix, StringComparison.OrdinalIgnoreCase)
            ? firmaImagenBase64[dataUrlPrefix.Length..]
            : firmaImagenBase64;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw;
        }

        if (bytes.Length == 0)
        {
            return null;
        }

        await using var stream = new MemoryStream(bytes);
        var storedFile = await fileStorage.SaveAsync(
            "firmas",
            $"{idDocumento:N}-{idFirmante:N}.png",
            stream,
            cancellationToken);

        return storedFile.Path;
    }

    private static string NormalizarMetodoFirma(string? metodo)
    {
        return string.Equals(metodo, "huella", StringComparison.OrdinalIgnoreCase) ? "huella" : "firma";
    }

    private async Task<TokenFirmaStatus> EvaluarSolicitudAsync(
        SolicitudFirma? solicitud,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken)
    {
        if (solicitud is null)
        {
            return TokenFirmaStatus.NoEncontrado;
        }

        var now = DateTime.UtcNow;
        if (solicitud.FechaVencimiento <= now)
        {
            solicitud.Estado = "vencido";
            if (solicitud.Documento is not null &&
                solicitud.Documento.Estado is not DocumentoEstados.Firmado and not DocumentoEstados.Anulado)
            {
                solicitud.Documento.Estado = DocumentoEstados.Vencido;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return TokenFirmaStatus.Vencido;
        }

        if (solicitud.FechaUso is not null || solicitud.Estado == "utilizado")
        {
            return TokenFirmaStatus.YaUtilizado;
        }

        if (solicitud.Documento is null ||
            solicitud.Documento.Estado is DocumentoEstados.Anulado or DocumentoEstados.Error or DocumentoEstados.Rechazado)
        {
            return TokenFirmaStatus.DocumentoNoDisponible;
        }

        return TokenFirmaStatus.Valido;
    }

    private static string ObtenerMensajeStatus(TokenFirmaStatus status)
    {
        return status switch
        {
            TokenFirmaStatus.NoEncontrado => "Token de firma no encontrado.",
            TokenFirmaStatus.Vencido => "El link de firma esta vencido.",
            TokenFirmaStatus.YaUtilizado => "El link de firma ya fue utilizado.",
            TokenFirmaStatus.DocumentoNoDisponible => "El documento no esta disponible para firma.",
            _ => "Token valido."
        };
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

internal static class SolicitudFirmaExtensions
{
    public static void SolicitudFirmaFechaUsoOrNow(this SolicitudFirma solicitud, DateTime now)
    {
        solicitud.FechaUso ??= now;
    }
}
