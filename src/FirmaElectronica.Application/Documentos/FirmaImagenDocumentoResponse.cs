namespace FirmaElectronica.Application.Documentos;

public sealed class FirmaImagenDocumentoResponse
{
    public Stream Content { get; init; } = Stream.Null;
    public string FileName { get; init; } = "firma.png";
    public string ContentType { get; init; } = "image/png";
}
