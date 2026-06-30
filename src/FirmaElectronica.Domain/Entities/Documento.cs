namespace FirmaElectronica.Domain.Entities;

public sealed class Documento
{
    public Guid IdDocumento { get; set; }
    public Guid IdEmpresa { get; set; }
    public string? IdSistemaOrigen { get; set; }
    public string? IdComprobanteOrigen { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    public string HashOriginal { get; set; } = string.Empty;
    public string? HashFirmado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaFirma { get; set; }
    public string RutaPdfOriginal { get; set; } = string.Empty;
    public string? RutaPdfFirmado { get; set; }
    public string? RutaConstancia { get; set; }
    public string? CreadoPor { get; set; }
    public string? UltimoError { get; set; }

    public Empresa? Empresa { get; set; }
    public ICollection<Firmante> Firmantes { get; set; } = new List<Firmante>();
    public ICollection<SolicitudFirma> SolicitudesFirma { get; set; } = new List<SolicitudFirma>();
    public ICollection<EventoAuditoria> EventosAuditoria { get; set; } = new List<EventoAuditoria>();
}
