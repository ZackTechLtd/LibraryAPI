using System.Text.RegularExpressions;
using Common.Util;

namespace LibraryAPIApp.Tests.Unit;

public class RandomKeyGeneratorTests
{
    private readonly RandomKeyGenerator _generator = new();

    [Fact]
    public void GetUniqueKey_ReturnsRequestedLength()
    {
        var key = _generator.GetUniqueKey(20);

        Assert.Equal(20, key.Length);
    }

    [Fact]
    public void GetUniqueKey_OnlyContainsAlphanumericCharacters()
    {
        var key = _generator.GetUniqueKey(100);

        Assert.Matches(new Regex("^[a-zA-Z0-9]+$"), key);
    }

    [Fact]
    public void CreateEmbededCustomerKey_RoundTripsNineCharacterCode()
    {
        const string secret = "ABCDEFGHI";

        var customerKey = _generator.CreateEmbededCustomerKey(secret);
        var extracted = _generator.GetEmbededCode(customerKey);

        Assert.Equal(secret, extracted);
    }

    [Fact]
    public void GetEmbededCode_WithNonNumericPrefix_ReturnsNull()
    {
        var result = _generator.GetEmbededCode("abc12345678");

        Assert.Null(result);
    }
}