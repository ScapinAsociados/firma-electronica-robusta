namespace FirmaElectronica.Application.Documentos;

public sealed class DocumentoNotFoundException(Guid idDocumento)
    : Exception($"No existe el documento '{idDocumento}'.")
{
    public Guid IdDocumento { get; } = idDocumento;
}
