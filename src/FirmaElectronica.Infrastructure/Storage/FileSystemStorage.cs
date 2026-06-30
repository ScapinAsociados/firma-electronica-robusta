using FirmaElectronica.Application.Storage;
using FirmaElectronica.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace FirmaElectronica.Infrastructure.Storage;

public sealed class FileSystemStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly string _basePath = Path.GetFullPath(options.Value.BasePath);

    public async Task<StoredFileResult> SaveAsync(
        string container,
        string fileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        var safeContainer = SanitizePathSegment(container);
        var safeFileName = SanitizeFileName(fileName);
        var directory = Path.Combine(_basePath, safeContainer);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, safeFileName);
        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return new StoredFileResult
        {
            Path = fullPath
        };
    }

    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La ruta solicitada esta fuera del storage configurado.");
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    private static string SanitizePathSegment(string value)
    {
        var invalid = Path.GetInvalidPathChars();
        var sanitized = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized;
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? $"{Guid.NewGuid():N}.pdf" : sanitized;
    }
}
