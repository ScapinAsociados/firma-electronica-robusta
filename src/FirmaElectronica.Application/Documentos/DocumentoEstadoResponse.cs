namespace FirmaElectronica.Application.Documentos;

public sealed class DocumentoEstadoResponse
{
    public Guid IdDocumento { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaAlta { get; init; }
    public DateTime FechaVencimiento { get; init; }
    public DateTime? FechaFirma { get; init; }
    public string? UltimoError { get; init; }
}
