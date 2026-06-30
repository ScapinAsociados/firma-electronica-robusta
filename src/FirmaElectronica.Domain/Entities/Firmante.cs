namespace FirmaElectronica.Domain.Entities;

public sealed class Firmante
{
    public Guid IdFirmante { get; set; }
    public Guid IdDocumento { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CUIT_DNI { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public int OrdenFirma { get; set; }
    public string EstadoFirma { get; set; } = "pendiente";
    public DateTime? FechaVista { get; set; }
    public DateTime? FechaFirma { get; set; }
    public string? RutaFirmaImagen { get; set; }

    public Documento? Documento { get; set; }
    public ICollection<SolicitudFirma> SolicitudesFirma { get; set; } = new List<SolicitudFirma>();
}
