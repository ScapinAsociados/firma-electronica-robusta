namespace FirmaElectronica.Application.Auth;

public sealed class ApiClientContext
{
    public Guid IdUsuarioApi { get; init; }
    public Guid IdEmpresa { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Permisos { get; init; } = string.Empty;
}
