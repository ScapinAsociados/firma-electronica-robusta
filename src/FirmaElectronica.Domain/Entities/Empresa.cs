namespace FirmaElectronica.Domain.Entities;

public sealed class Empresa
{
    public Guid IdEmpresa { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string CUIT { get; set; } = string.Empty;
    public string? Dominio { get; set; }
    public string? LogoUrl { get; set; }
    public string? ColorPrincipal { get; set; }
    public string Estado { get; set; } = "activa";
    public DateTime FechaAlta { get; set; }

    public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    public ICollection<UsuarioApi> UsuariosApi { get; set; } = new List<UsuarioApi>();
}
