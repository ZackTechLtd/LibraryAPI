using LibraryAPIApp.Util;

namespace LibraryAPIApp.Tests.Unit;

public class JwtSecurityKeyTests
{
    [Fact]
    public void Create_ShortSecret_ProducesKeyOfAtLeast256Bits()
    {
        // Regression: the legacy 17-char secret is 136 bits, below the 256-bit HS256 minimum.
        var key = JwtSecurityKey.Create("ZackTechSecretKey");

        Assert.Equal(256, key.KeySize);
    }

    [Fact]
    public void Create_SameSecret_ProducesSameKeyBytes()
    {
        var first = JwtSecurityKey.Create("same-secret");
        var second = JwtSecurityKey.Create("same-secret");

        Assert.Equal(first.Key, second.Key);
    }

    [Fact]
    public void Create_DifferentSecrets_ProduceDifferentKeyBytes()
    {
        var first = JwtSecurityKey.Create("secret-one");
        var second = JwtSecurityKey.Create("secret-two");

        Assert.NotEqual(first.Key, second.Key);
    }
}