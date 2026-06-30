using FirmaElectronica.Domain.Entities;
using FirmaElectronica.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FirmaElectronica.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    public static readonly Guid DefaultEmpresaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string DefaultApiKey = "dev-firma-electronica-api-key";

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FirmaElectronicaDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        var empresaExists = await dbContext.Empresas.AnyAsync(x => x.IdEmpresa == DefaultEmpresaId, cancellationToken);
        if (!empresaExists)
        {
            dbContext.Empresas.Add(new Empresa
            {
                IdEmpresa = DefaultEmpresaId,
                RazonSocial = "Empresa Demo",
                CUIT = "30-00000000-0",
                Dominio = "localhost",
                ColorPrincipal = "#2563eb",
                Estado = "activa",
                FechaAlta = DateTime.UtcNow
            });
        }

        var tokenService = new SecureTokenService();
        var apiKeyHash = tokenService.HashToken(DefaultApiKey);
        var usuarioExists = await dbContext.UsuariosApi.AnyAsync(x => x.ApiKeyHash == apiKeyHash, cancellationToken);
        if (!usuarioExists)
        {
            dbContext.UsuariosApi.Add(new UsuarioApi
            {
                IdUsuarioApi = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IdEmpresa = DefaultEmpresaId,
                Nombre = "Access Demo",
                ApiKeyHash = apiKeyHash,
                Permisos = "documentos:crear documentos:estado",
                Estado = "activo",
                FechaAlta = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
