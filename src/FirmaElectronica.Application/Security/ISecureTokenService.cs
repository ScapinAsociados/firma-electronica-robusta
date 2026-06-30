namespace FirmaElectronica.Application.Security;

public interface ISecureTokenService
{
    SecureTokenResult GenerateToken();
    string HashToken(string token);
}
