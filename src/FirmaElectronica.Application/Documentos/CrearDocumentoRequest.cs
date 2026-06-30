namespace FirmaElectronica.Application.Documentos;

public sealed class CrearDocumentoRequest
{
    public Guid IdEmpresa { get; init; }
    public string? IdSistemaOrigen { get; init; }
    public string? IdComprobanteOrigen { get; init; }
    public string TipoDocumento { get; init; } = string.Empty;
    public string NombreFirmante { get; init; } = string.Empty;
    public string? CuitDniFirmante { get; init; }
    public string EmailFirmante { get; init; } = string.Empty;
    public string? CelularFirmante { get; init; }
    public string? CreadoPor { get; init; }
    public string NombreArchivo { get; init; } = string.Empty;
    public Stream PdfStream { get; init; } = Stream.Null;
}
