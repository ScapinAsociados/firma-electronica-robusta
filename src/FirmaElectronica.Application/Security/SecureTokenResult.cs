namespace FirmaElectronica.Application.Security;

public sealed class SecureTokenResult
{
    public string Token { get; init; } = string.Empty;
    public string TokenHash { get; init; } = string.Empty;
}
