namespace FirmaElectronica.Application.Auth;

public interface IApiKeyValidator
{
    Task<ApiClientContext?> ValidateAsync(string apiKey, CancellationToken cancellationToken);
}
