namespace FirmaElectronica.Application.Hashing;

public interface IPdfHashService
{
    Task<string> ComputeSha256Async(Stream pdfStream, CancellationToken cancellationToken);
}
