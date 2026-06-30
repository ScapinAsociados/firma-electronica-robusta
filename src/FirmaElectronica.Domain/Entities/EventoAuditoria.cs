namespace FirmaElectronica.Domain.Entities;

public sealed class EventoAuditoria
{
    public Guid IdEvento { get; set; }
    public Guid IdDocumento { get; set; }
    public Guid? IdFirmante { get; set; }
    public DateTime FechaHoraUTC { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? IP { get; set; }
    public string? UserAgent { get; set; }
    public string? HashDocumentoEnEvento { get; set; }
    public string? DatosJson { get; set; }

    public Documento? Documento { get; set; }
    public Firmante? Firmante { get; set; }
}
