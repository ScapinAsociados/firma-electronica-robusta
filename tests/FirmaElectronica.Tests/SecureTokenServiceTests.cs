using FirmaElectronica.Infrastructure.Security;

namespace FirmaElectronica.Tests;

public sealed class SecureTokenServiceTests
{
    [Fact]
    public void GenerateToken_ReturnsTokenAndOnlyHashIsPersistable()
    {
        var service = new SecureTokenService();

        var result = service.GenerateToken();

        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.TokenHash);
        Assert.NotEqual(result.Token, result.TokenHash);
        Assert.Equal(64, result.TokenHash.Length);
        Assert.Equal(result.TokenHash, service.HashToken(result.Token));
    }
}
