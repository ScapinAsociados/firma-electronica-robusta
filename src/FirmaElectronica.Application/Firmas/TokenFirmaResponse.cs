namespace FirmaElectronica.Application.Firmas;

public sealed class TokenFirmaResponse
{
    public TokenFirmaStatus Status { get; init; }
    public Guid? IdDocumento { get; init; }
    public string? EstadoDocumento { get; init; }
    public DateTime? FechaVencimiento { get; init; }
}
