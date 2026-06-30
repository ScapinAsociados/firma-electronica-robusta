using FirmaElectronica.Application.Auth;
using FirmaElectronica.Application.Documentos;
using FirmaElectronica.Application.Firmas;
using FirmaElectronica.Application.Hashing;
using FirmaElectronica.Application.Security;
using FirmaElectronica.Application.Storage;
using FirmaElectronica.Infrastructure.Auth;
using FirmaElectronica.Infrastructure.Documentos;
using FirmaElectronica.Infrastructure.Firmas;
using FirmaElectronica.Infrastructure.Hashing;
using FirmaElectronica.Infrastructure.Options;
using FirmaElectronica.Infrastructure.Persistence;
using FirmaElectronica.Infrastructure.Security;
using FirmaElectronica.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FirmaElectronica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(options =>
        {
            options.BasePath = configuration["FileStorage:BasePath"] ?? options.BasePath;
        });
        services.Configure<FirmaLinksOptions>(options =>
        {
            options.BaseUrl = configuration["FirmaLinks:BaseUrl"] ?? options.BaseUrl;
            if (int.TryParse(configuration["FirmaLinks:ExpirationHours"], out var expirationHours))
            {
                options.ExpirationHours = expirationHours;
            }
        });

        services.AddDbContext<FirmaElectronicaDbContext>(options =>
        {
            if (string.Equals(configuration["Database:Provider"], "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase(configuration["Database:Name"] ?? "FirmaElectronicaRobusta_Dev");
                return;
            }

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IDocumentosService, DocumentosService>();
        services.AddScoped<IFirmaService, FirmaService>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
        services.AddScoped<IFileStorage, FileSystemStorage>();
        services.AddSingleton<IPdfHashService, PdfHashService>();
        services.AddSingleton<ISecureTokenService, SecureTokenService>();

        return services;
    }
}
