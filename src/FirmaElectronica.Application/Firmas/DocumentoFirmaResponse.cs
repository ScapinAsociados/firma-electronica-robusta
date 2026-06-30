namespace FirmaElectronica.Application.Firmas;

public sealed class DocumentoFirmaResponse
{
    public Guid IdDocumento { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string TipoDocumento { get; init; } = string.Empty;
    public string NombreArchivoOriginal { get; init; } = string.Empty;
    public string HashOriginal { get; init; } = string.Empty;
    public DateTime FechaVencimiento { get; init; }
    public FirmanteFirmaResponse Firmante { get; init; } = new();
}
