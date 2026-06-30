namespace FirmaElectronica.Application.Storage;

public interface IFileStorage
{
    Task<StoredFileResult> SaveAsync(
        string container,
        string fileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken);
}
