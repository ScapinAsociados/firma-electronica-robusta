namespace FirmaElectronica.Domain.Entities;

public sealed class UsuarioApi
{
    public Guid IdUsuarioApi { get; set; }
    public Guid IdEmpresa { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public string Permisos { get; set; } = string.Empty;
    public string Estado { get; set; } = "activo";
    public DateTime FechaAlta { get; set; }
    public DateTime? UltimoUso { get; set; }

    public Empresa? Empresa { get; set; }
}
