using System.Security.Cryptography;
using FirmaElectronica.Application.Hashing;

namespace FirmaElectronica.Infrastructure.Hashing;

public sealed class PdfHashService : IPdfHashService
{
    public async Task<string> ComputeSha256Async(Stream pdfStream, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(pdfStream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
