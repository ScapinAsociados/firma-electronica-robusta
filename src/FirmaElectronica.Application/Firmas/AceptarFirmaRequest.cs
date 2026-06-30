namespace FirmaElectronica.Application.Firmas;

public sealed class AceptarFirmaRequest
{
    public bool AceptaTerminos { get; init; }
    public string TextoAceptado { get; init; } = string.Empty;
    public string? NombreConfirmado { get; init; }
    public string? CuitDniConfirmado { get; init; }
    public string? FirmaImagenBase64 { get; init; }
    public string? FirmaMetodo { get; init; }
    public int FirmaPuntosCapturados { get; init; }
}
