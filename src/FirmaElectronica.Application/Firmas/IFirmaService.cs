using FirmaElectronica.Application.Documentos;

namespace FirmaElectronica.Application.Firmas;

public interface IFirmaService
{
    Task<TokenFirmaResponse> ValidarTokenAsync(
        string token,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken);

    Task<FirmaOperacionResult<DocumentoFirmaResponse>> ObtenerDocumentoAsync(
        string token,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken);

    Task<FirmaOperacionResult<PdfOriginalResponse>> ObtenerPdfOriginalAsync(
        string token,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken);

    Task<FirmaOperacionResult<FirmaAceptadaResponse>> AceptarAsync(
        string token,
        AceptarFirmaRequest request,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken);
}
