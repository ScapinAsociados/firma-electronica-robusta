namespace FirmaElectronica.Domain.Entities;

public sealed class SolicitudFirma
{
    public Guid IdSolicitud { get; set; }
    public Guid IdDocumento { get; set; }
    public Guid IdFirmante { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaUso { get; set; }
    public int Intentos { get; set; }
    public string Estado { get; set; } = "pendiente";

    public Documento? Documento { get; set; }
    public Firmante? Firmante { get; set; }
}
