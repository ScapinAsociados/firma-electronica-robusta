using FirmaElectronica.Application.Auth;
using FirmaElectronica.Application.Documentos;
using FirmaElectronica.Application.Firmas;
using FirmaElectronica.Infrastructure;
using FirmaElectronica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/documentos", async Task<Results<Created<CrearDocumentoResponse>, BadRequest<ErrorResponse>, UnauthorizedHttpResult>> (
    [FromForm] CrearDocumentoForm form,
    HttpContext httpContext,
    IApiKeyValidator apiKeyValidator,
    IDocumentosService documentosService,
    CancellationToken cancellationToken) =>
{
    var apiClient = await AuthenticateApiClientAsync(httpContext, apiKeyValidator, cancellationToken);
    if (apiClient is null)
    {
        return TypedResults.Unauthorized();
    }

    var formError = ValidateForm(form);
    if (formError is not null)
    {
        return TypedResults.BadRequest(new ErrorResponse(formError));
    }

    var validationError = await ValidatePdfAsync(form.Archivo, cancellationToken);
    if (validationError is not null)
    {
        return TypedResults.BadRequest(new ErrorResponse(validationError));
    }

    await using var stream = form.Archivo.OpenReadStream();
    var response = await documentosService.CrearAsync(
        new CrearDocumentoRequest
        {
            IdEmpresa = apiClient.IdEmpresa,
            IdSistemaOrigen = form.IdSistemaOrigen,
            IdComprobanteOrigen = form.IdComprobanteOrigen,
            TipoDocumento = form.TipoDocumento,
            NombreFirmante = form.NombreFirmante,
            CuitDniFirmante = form.CuitDniFirmante,
            EmailFirmante = form.EmailFirmante,
            CelularFirmante = form.CelularFirmante,
            CreadoPor = form.CreadoPor,
            NombreArchivo = form.Archivo.FileName,
            PdfStream = stream
        },
        BuildAuditoriaRequest(httpContext),
        cancellationToken);

    return TypedResults.Created($"/api/documentos/{response.IdDocumento}", response);
})
.DisableAntiforgery()
.Accepts<CrearDocumentoForm>("multipart/form-data")
.WithName("CrearDocumento")
.WithOpenApi();

app.MapGet("/api/documentos/{id:guid}/estado", async Task<Results<Ok<DocumentoEstadoResponse>, NotFound, UnauthorizedHttpResult>> (
    Guid id,
    HttpContext httpContext,
    IApiKeyValidator apiKeyValidator,
    IDocumentosService documentosService,
    CancellationToken cancellationToken) =>
{
    var apiClient = await AuthenticateApiClientAsync(httpContext, apiKeyValidator, cancellationToken);
    if (apiClient is null)
    {
        return TypedResults.Unauthorized();
    }

    var response = await documentosService.ObtenerEstadoAsync(id, apiClient.IdEmpresa, cancellationToken);
    return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
})
.WithName("ObtenerEstadoDocumento")
.WithOpenApi();

app.MapGet("/api/documentos/{id:guid}/firma-imagen", async Task<Results<FileStreamHttpResult, NotFound, UnauthorizedHttpResult>> (
    Guid id,
    HttpContext httpContext,
    IApiKeyValidator apiKeyValidator,
    IDocumentosService documentosService,
    CancellationToken cancellationToken) =>
{
    var apiClient = await AuthenticateApiClientAsync(httpContext, apiKeyValidator, cancellationToken);
    if (apiClient is null)
    {
        return TypedResults.Unauthorized();
    }

    var response = await documentosService.ObtenerFirmaImagenAsync(id, apiClient.IdEmpresa, cancellationToken);
    return response is null
        ? TypedResults.NotFound()
        : TypedResults.File(response.Content, response.ContentType, response.FileName);
})
.WithName("ObtenerFirmaImagenDocumento")
.WithOpenApi();

app.MapGet("/f/{token}", (IWebHostEnvironment environment) =>
{
    var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    var filePath = Path.Combine(webRoot, "firma.html");
    return TypedResults.PhysicalFile(filePath, "text/html; charset=utf-8");
})
.WithName("PortalFirma");

app.MapGet("/api/firmas/{token}/validar", async Task<Results<Ok<TokenFirmaResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, Conflict<ErrorResponse>>> (
    string token,
    HttpContext httpContext,
    IFirmaService firmaService,
    CancellationToken cancellationToken) =>
{
    var response = await firmaService.ValidarTokenAsync(token, BuildAuditoriaRequest(httpContext), cancellationToken);
    return response.Status switch
    {
        TokenFirmaStatus.Valido => TypedResults.Ok(response),
        TokenFirmaStatus.NoEncontrado => TypedResults.NotFound(new ErrorResponse("Token de firma no encontrado.")),
        TokenFirmaStatus.Vencido => TypedResults.BadRequest(new ErrorResponse("El link de firma esta vencido.")),
        TokenFirmaStatus.YaUtilizado => TypedResults.Conflict(new ErrorResponse("El link de firma ya fue utilizado.")),
        _ => TypedResults.Conflict(new ErrorResponse("El link de firma no esta disponible."))
    };
})
.WithName("ValidarTokenFirma")
.WithOpenApi();

app.MapGet("/api/firmas/{token}/documento", async Task<Results<Ok<DocumentoFirmaResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, Conflict<ErrorResponse>>> (
    string token,
    HttpContext httpContext,
    IFirmaService firmaService,
    CancellationToken cancellationToken) =>
{
    var result = await firmaService.ObtenerDocumentoAsync(token, BuildAuditoriaRequest(httpContext), cancellationToken);
    return MapFirmaResult(result);
})
.WithName("ObtenerDocumentoFirma")
.WithOpenApi();

app.MapGet("/api/firmas/{token}/pdf", async Task<Results<FileStreamHttpResult, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, Conflict<ErrorResponse>>> (
    string token,
    HttpContext httpContext,
    IFirmaService firmaService,
    CancellationToken cancellationToken) =>
{
    var result = await firmaService.ObtenerPdfOriginalAsync(token, BuildAuditoriaRequest(httpContext), cancellationToken);
    if (result.Succeeded && result.Value is not null)
    {
        return TypedResults.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    var error = new ErrorResponse(result.Error ?? "No se pudo obtener el PDF original.");
    return result.Status switch
    {
        TokenFirmaStatus.NoEncontrado => TypedResults.NotFound(error),
        TokenFirmaStatus.Vencido => TypedResults.BadRequest(error),
        TokenFirmaStatus.DocumentoNoDisponible => TypedResults.BadRequest(error),
        _ => TypedResults.Conflict(error)
    };
})
.WithName("ObtenerPdfOriginalFirma")
.WithOpenApi();

app.MapPost("/api/firmas/{token}/aceptar", async Task<Results<Ok<FirmaAceptadaResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, Conflict<ErrorResponse>>> (
    string token,
    AceptarFirmaRequest request,
    HttpContext httpContext,
    IFirmaService firmaService,
    CancellationToken cancellationToken) =>
{
    var result = await firmaService.AceptarAsync(token, request, BuildAuditoriaRequest(httpContext), cancellationToken);
    return MapFirmaResult(result);
})
.WithName("AceptarFirma")
.WithOpenApi();

app.Run();

static AuditoriaRequest BuildAuditoriaRequest(HttpContext httpContext)
{
    return new AuditoriaRequest
    {
        Ip = httpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent = httpContext.Request.Headers["User-Agent"].ToString()
    };
}

static async Task<ApiClientContext?> AuthenticateApiClientAsync(
    HttpContext httpContext,
    IApiKeyValidator apiKeyValidator,
    CancellationToken cancellationToken)
{
    if (!httpContext.Request.Headers.TryGetValue("X-API-Key", out var values))
    {
        return null;
    }

    return await apiKeyValidator.ValidateAsync(values.ToString(), cancellationToken);
}

static string? ValidateForm(CrearDocumentoForm form)
{
    if (string.IsNullOrWhiteSpace(form.TipoDocumento))
    {
        return "Debe indicar el tipo de documento.";
    }

    if (string.IsNullOrWhiteSpace(form.NombreFirmante))
    {
        return "Debe indicar el nombre del firmante.";
    }

    if (string.IsNullOrWhiteSpace(form.EmailFirmante))
    {
        return "Debe indicar el email del firmante.";
    }

    return null;
}

static async Task<string?> ValidatePdfAsync(IFormFile? archivo, CancellationToken cancellationToken)
{
    const long maxFileSize = 25 * 1024 * 1024;

    if (archivo is null || archivo.Length == 0)
    {
        return "Debe adjuntar un archivo PDF.";
    }

    if (archivo.Length > maxFileSize)
    {
        return "El PDF supera el tamano maximo permitido de 25 MB.";
    }

    if (!archivo.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(archivo.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
    {
        return "Solo se aceptan archivos PDF.";
    }

    await using var stream = archivo.OpenReadStream();
    var header = new byte[5];
    var read = await stream.ReadAsync(header, cancellationToken);
    if (read < header.Length || header[0] != '%' || header[1] != 'P' || header[2] != 'D' || header[3] != 'F' || header[4] != '-')
    {
        return "El archivo no tiene una cabecera PDF valida.";
    }

    return null;
}

static Results<Ok<T>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, Conflict<ErrorResponse>> MapFirmaResult<T>(
    FirmaOperacionResult<T> result)
{
    if (result.Succeeded && result.Value is not null)
    {
        return TypedResults.Ok(result.Value);
    }

    var error = new ErrorResponse(result.Error ?? "La operacion de firma no pudo completarse.");
    return result.Status switch
    {
        TokenFirmaStatus.NoEncontrado => TypedResults.NotFound(error),
        TokenFirmaStatus.Vencido => TypedResults.BadRequest(error),
        TokenFirmaStatus.DocumentoNoDisponible => TypedResults.BadRequest(error),
        _ => TypedResults.Conflict(error)
    };
}

public sealed class CrearDocumentoForm
{
    public string? IdSistemaOrigen { get; set; }
    public string? IdComprobanteOrigen { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NombreFirmante { get; set; } = string.Empty;
    public string? CuitDniFirmante { get; set; }
    public string EmailFirmante { get; set; } = string.Empty;
    public string? CelularFirmante { get; set; }
    public string? CreadoPor { get; set; }
    public IFormFile Archivo { get; set; } = default!;
}

public sealed record ErrorResponse(string Error);

public partial class Program;
