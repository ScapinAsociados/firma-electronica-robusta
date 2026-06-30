namespace FirmaElectronica.Infrastructure.Options;

public sealed class FirmaLinksOptions
{
    public string BaseUrl { get; set; } = "https://localhost:5001";
    public int ExpirationHours { get; set; } = 72;
}
