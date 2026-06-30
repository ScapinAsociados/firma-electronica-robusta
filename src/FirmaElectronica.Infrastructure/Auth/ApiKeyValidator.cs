using FirmaElectronica.Application.Auth;
using FirmaElectronica.Application.Security;
using FirmaElectronica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirmaElectronica.Infrastructure.Auth;

public sealed class ApiKeyValidator(
    FirmaElectronicaDbContext dbContext,
    ISecureTokenService secureTokenService) : IApiKeyValidator
{
    public async Task<ApiClientContext?> ValidateAsync(string apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var apiKeyHash = secureTokenService.HashToken(apiKey);
        var usuario = await dbContext.UsuariosApi
            .AsTracking()
            .SingleOrDefaultAsync(x => x.ApiKeyHash == apiKeyHash && x.Estado == "activo", cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        usuario.UltimoUso = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ApiClientContext
        {
            IdUsuarioApi = usuario.IdUsuarioApi,
            IdEmpresa = usuario.IdEmpresa,
            Nombre = usuario.Nombre,
            Permisos = usuario.Permisos
        };
    }
}
