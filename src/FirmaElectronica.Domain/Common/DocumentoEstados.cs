namespace FirmaElectronica.Domain.Common;

public static class DocumentoEstados
{
    public const string Borrador = "borrador";
    public const string PendienteEnvio = "pendiente_envio";
    public const string PendienteFirma = "pendiente_firma";
    public const string Visto = "visto";
    public const string Firmado = "firmado";
    public const string Rechazado = "rechazado";
    public const string Vencido = "vencido";
    public const string Anulado = "anulado";
    public const string Error = "error";
}
