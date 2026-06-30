namespace FirmaElectronica.Application.Firmas;

public sealed class FirmanteFirmaResponse
{
    public string Nombre { get; init; } = string.Empty;
    public string? CuitDni { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Celular { get; init; }
    public string EstadoFirma { get; init; } = string.Empty;
    public bool TieneFirmaImagen { get; init; }
}
