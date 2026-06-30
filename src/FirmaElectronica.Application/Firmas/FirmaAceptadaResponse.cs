namespace FirmaElectronica.Application.Firmas;

public sealed class FirmaAceptadaResponse
{
    public Guid IdDocumento { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaFirma { get; init; }
}
