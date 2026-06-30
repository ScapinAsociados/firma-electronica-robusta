using System.Security.Cryptography;
using System.Text;
using FirmaElectronica.Application.Security;

namespace FirmaElectronica.Infrastructure.Security;

public sealed class SecureTokenService : ISecureTokenService
{
    public SecureTokenResult GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64UrlEncode(bytes);

        return new SecureTokenResult
        {
            Token = token,
            TokenHash = HashToken(token)
        };
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
