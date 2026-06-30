namespace FirmaElectronica.Application.Documentos;

public interface IDocumentosService
{
    Task<CrearDocumentoResponse> CrearAsync(
        CrearDocumentoRequest request,
        AuditoriaRequest auditoria,
        CancellationToken cancellationToken);

    Task<DocumentoEstadoResponse?> ObtenerEstadoAsync(
        Guid idDocumento,
        Guid idEmpresa,
        CancellationToken cancellationToken);

    Task<FirmaImagenDocumentoResponse?> ObtenerFirmaImagenAsync(
        Guid idDocumento,
        Guid idEmpresa,
        CancellationToken cancellationToken);
}
