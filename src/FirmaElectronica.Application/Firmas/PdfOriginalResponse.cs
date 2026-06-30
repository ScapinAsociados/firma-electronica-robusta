namespace FirmaElectronica.Application.Firmas;

public sealed class PdfOriginalResponse
{
    public Stream Content { get; init; } = Stream.Null;
    public string FileName { get; init; } = "documento.pdf";
    public string ContentType { get; init; } = "application/pdf";
}
