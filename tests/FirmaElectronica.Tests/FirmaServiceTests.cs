using FirmaElectronica.Application.Documentos;
using FirmaElectronica.Application.Firmas;
using FirmaElectronica.Application.Storage;
using FirmaElectronica.Domain.Common;
using FirmaElectronica.Domain.Entities;
using FirmaElectronica.Infrastructure.Firmas;
using FirmaElectronica.Infrastructure.Persistence;
using FirmaElectronica.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FirmaElectronica.Tests;

public sealed class FirmaServiceTests
{
    [Fact]
    public async Task ValidarTokenAsync_WhenTokenIsValid_RegistersLinkOpenedWithoutPlainToken()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var token = await SeedSolicitudAsync(dbContext, tokenService, DateTime.UtcNow.AddHours(1));
        var service = new FirmaService(dbContext, tokenService, new TestFileStorage());

        var result = await service.ValidarTokenAsync(token, Auditoria(), CancellationToken.None);

        Assert.Equal(TokenFirmaStatus.Valido, result.Status);
        Assert.Contains(dbContext.EventosAuditoria, x => x.TipoEvento == AuditoriaEventos.LinkAbierto);
        Assert.DoesNotContain(token, dbContext.SolicitudesFirma.Select(x => x.TokenHash));
    }

    [Fact]
    public async Task ValidarTokenAsync_WhenTokenDoesNotExist_ReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var service = new FirmaService(dbContext, new SecureTokenService(), new TestFileStorage());

        var result = await service.ValidarTokenAsync("token-inexistente", Auditoria(), CancellationToken.None);

        Assert.Equal(TokenFirmaStatus.NoEncontrado, result.Status);
    }

    [Fact]
    public async Task ValidarTokenAsync_WhenTokenIsExpired_MarksRequestAndDocumentAsExpired()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var token = await SeedSolicitudAsync(dbContext, tokenService, DateTime.UtcNow.AddMinutes(-5));
        var service = new FirmaService(dbContext, tokenService, new TestFileStorage());

        var result = await service.ValidarTokenAsync(token, Auditoria(), CancellationToken.None);

        Assert.Equal(TokenFirmaStatus.Vencido, result.Status);
        Assert.Equal("vencido", dbContext.SolicitudesFirma.Single().Estado);
        Assert.Equal(DocumentoEstados.Vencido, dbContext.Documentos.Single().Estado);
    }

    [Fact]
    public async Task AceptarAsync_WhenTermsAreAccepted_MarksDocumentAsSigned()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var token = await SeedSolicitudAsync(dbContext, tokenService, DateTime.UtcNow.AddHours(1));
        var service = new FirmaService(dbContext, tokenService, new TestFileStorage());

        var result = await service.AceptarAsync(
            token,
            new AceptarFirmaRequest
            {
                AceptaTerminos = true,
                TextoAceptado = "Acepto firmar electronicamente el documento.",
                FirmaImagenBase64 = "data:image/png;base64," + Convert.ToBase64String("%PNG test"u8.ToArray()),
                FirmaMetodo = "firma",
                FirmaPuntosCapturados = 8
            },
            Auditoria(),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(DocumentoEstados.Firmado, dbContext.Documentos.Single().Estado);
        Assert.Equal("firmado", dbContext.Firmantes.Single().EstadoFirma);
        Assert.NotNull(dbContext.Firmantes.Single().RutaFirmaImagen);
        Assert.NotNull(dbContext.SolicitudesFirma.Single().FechaUso);
        Assert.Contains(dbContext.EventosAuditoria, x => x.TipoEvento == AuditoriaEventos.TerminosAceptados);
        Assert.Contains(dbContext.EventosAuditoria, x => x.TipoEvento == AuditoriaEventos.FirmaConfirmada);
    }

    [Fact]
    public async Task AceptarAsync_WhenTokenWasAlreadyUsed_DoesNotSignAgain()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var token = await SeedSolicitudAsync(dbContext, tokenService, DateTime.UtcNow.AddHours(1));
        var service = new FirmaService(dbContext, tokenService, new TestFileStorage());
        var request = new AceptarFirmaRequest
        {
            AceptaTerminos = true,
            TextoAceptado = "Acepto firmar electronicamente el documento.",
            FirmaImagenBase64 = "data:image/png;base64," + Convert.ToBase64String("%PNG test"u8.ToArray()),
            FirmaMetodo = "firma",
            FirmaPuntosCapturados = 8
        };

        var first = await service.AceptarAsync(token, request, Auditoria(), CancellationToken.None);
        var second = await service.AceptarAsync(token, request, Auditoria(), CancellationToken.None);

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Equal(TokenFirmaStatus.YaUtilizado, second.Status);
        Assert.Single(dbContext.EventosAuditoria.Where(x => x.TipoEvento == AuditoriaEventos.FirmaConfirmada));
    }

    [Fact]
    public async Task AceptarAsync_WhenSignatureImageIsMissing_DoesNotMarkDocumentAsSigned()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var token = await SeedSolicitudAsync(dbContext, tokenService, DateTime.UtcNow.AddHours(1));
        var service = new FirmaService(dbContext, tokenService, new TestFileStorage());

        var result = await service.AceptarAsync(
            token,
            new AceptarFirmaRequest
            {
                AceptaTerminos = true,
                TextoAceptado = "Acepto firmar electronicamente el documento."
            },
            Auditoria(),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(DocumentoEstados.PendienteFirma, dbContext.Documentos.Single().Estado);
        Assert.Null(dbContext.Firmantes.Single().RutaFirmaImagen);
    }

    [Fact]
    public async Task ObtenerPdfOriginalAsync_WhenTokenIsValid_ReturnsPdfAndRegistersPdfViewed()
    {
        await using var dbContext = CreateDbContext();
        var tokenService = new SecureTokenService();
        var token = await SeedSolicitudAsync(dbContext, tokenService, DateTime.UtcNow.AddHours(1));
        var service = new FirmaService(dbContext, tokenService, new TestFileStorage());

        var result = await service.ObtenerPdfOriginalAsync(token, Auditoria(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("application/pdf", result.Value?.ContentType);
        Assert.Contains(dbContext.EventosAuditoria, x => x.TipoEvento == AuditoriaEventos.PdfVisualizado);
        Assert.Equal(DocumentoEstados.Visto, dbContext.Documentos.Single().Estado);
    }

    private static FirmaElectronicaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FirmaElectronicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FirmaElectronicaDbContext(options);
    }

    private static async Task<string> SeedSolicitudAsync(
        FirmaElectronicaDbContext dbContext,
        SecureTokenService tokenService,
        DateTime fechaVencimiento)
    {
        var empresaId = Guid.NewGuid();
        var documentoId = Guid.NewGuid();
        var firmanteId = Guid.NewGuid();
        var token = tokenService.GenerateToken();

        dbContext.Empresas.Add(new Empresa
        {
            IdEmpresa = empresaId,
            RazonSocial = "Empresa Test",
            CUIT = Guid.NewGuid().ToString("N")[..11],
            Estado = "activa",
            FechaAlta = DateTime.UtcNow
        });
        dbContext.Documentos.Add(new Documento
        {
            IdDocumento = documentoId,
            IdEmpresa = empresaId,
            TipoDocumento = "comprobante",
            NombreArchivoOriginal = "documento.pdf",
            HashOriginal = new string('a', 64),
            Estado = DocumentoEstados.PendienteFirma,
            FechaAlta = DateTime.UtcNow,
            FechaVencimiento = fechaVencimiento,
            RutaPdfOriginal = "storage/documento.pdf"
        });
        dbContext.Firmantes.Add(new Firmante
        {
            IdFirmante = firmanteId,
            IdDocumento = documentoId,
            Nombre = "Cliente Test",
            Email = "cliente@test.local",
            OrdenFirma = 1,
            EstadoFirma = "pendiente"
        });
        dbContext.SolicitudesFirma.Add(new SolicitudFirma
        {
            IdSolicitud = Guid.NewGuid(),
            IdDocumento = documentoId,
            IdFirmante = firmanteId,
            TokenHash = token.TokenHash,
            FechaCreacion = DateTime.UtcNow,
            FechaVencimiento = fechaVencimiento,
            Estado = "pendiente"
        });

        await dbContext.SaveChangesAsync();
        return token.Token;
    }

    private static AuditoriaRequest Auditoria() => new()
    {
        Ip = "127.0.0.1",
        UserAgent = "tests"
    };

    private sealed class TestFileStorage : IFileStorage
    {
        public Task<StoredFileResult> SaveAsync(
            string container,
            string fileName,
            Stream content,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new StoredFileResult { Path = $"storage/{container}/{fileName}" });
        }

        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken)
        {
            Stream stream = new MemoryStream("%PDF- test"u8.ToArray());
            return Task.FromResult(stream);
        }
    }
}
