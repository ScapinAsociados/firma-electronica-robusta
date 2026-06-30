namespace FirmaElectronica.Application.Firmas;

public sealed class FirmaOperacionResult<T>
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public TokenFirmaStatus Status { get; init; }
    public T? Value { get; init; }

    public static FirmaOperacionResult<T> Success(T value) => new()
    {
        Succeeded = true,
        Status = TokenFirmaStatus.Valido,
        Value = value
    };

    public static FirmaOperacionResult<T> Failure(TokenFirmaStatus status, string error) => new()
    {
        Succeeded = false,
        Status = status,
        Error = error
    };
}
