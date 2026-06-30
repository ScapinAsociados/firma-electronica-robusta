namespace FirmaElectronica.Application.Documentos;

public sealed class CrearDocumentoResponse
{
    public Guid IdDocumento { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string UrlFirma { get; init; } = string.Empty;
    public string HashOriginal { get; init; } = string.Empty;
    public DateTime FechaVencimiento { get; init; }
}
