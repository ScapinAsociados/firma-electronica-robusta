using FirmaElectronica.Domain.Entities;
using FirmaElectronica.Infrastructure.Auth;
using FirmaElectronica.Infrastructure.Persistence;
using FirmaElectronica.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FirmaElectronica.Tests;

public sealed class ApiKeyValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenApiKeyMatchesHash_ReturnsClientContext()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var apiKey = "test-api-key";
        var empresaId = Guid.NewGuid();

        dbContext.Empresas.Add(new Empresa
        {
            IdEmpresa = empresaId,
            RazonSocial = "Empresa Test",
            CUIT = "30-12345678-9",
            Estado = "activa",
            FechaAlta = DateTime.UtcNow
        });
        dbContext.UsuariosApi.Add(new UsuarioApi
        {
            IdUsuarioApi = Guid.NewGuid(),
            IdEmpresa = empresaId,
            Nombre = "Access Test",
            ApiKeyHash = tokenService.HashToken(apiKey),
            Permisos = "documentos:crear",
            Estado = "activo",
            FechaAlta = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var validator = new ApiKeyValidator(dbContext, tokenService);

        var result = await validator.ValidateAsync(apiKey, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(empresaId, result.IdEmpresa);
        Assert.DoesNotContain(apiKey, dbContext.UsuariosApi.Select(x => x.ApiKeyHash));
    }

    [Fact]
    public async Task ValidateAsync_WhenApiKeyDoesNotMatch_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var validator = new ApiKeyValidator(dbContext, new SecureTokenService());

        var result = await validator.ValidateAsync("wrong-key", CancellationToken.None);

        Assert.Null(result);
    }

    private static FirmaElectronicaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FirmaElectronicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FirmaElectronicaDbContext(options);
    }
}
