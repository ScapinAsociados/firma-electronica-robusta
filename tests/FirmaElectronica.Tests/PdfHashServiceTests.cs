using System.Text;
using FirmaElectronica.Infrastructure.Hashing;

namespace FirmaElectronica.Tests;

public sealed class PdfHashServiceTests
{
    [Fact]
    public async Task ComputeSha256Async_ReturnsExpectedLowercaseHexHash()
    {
        var service = new PdfHashService();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("%PDF- test"));

        var hash = await service.ComputeSha256Async(stream, CancellationToken.None);

        Assert.Equal("228b1d474ec7311300fa121a332c4cc8ee1b0afe03aa8fc5ddceb236bed1a4c3", hash);
    }
}
