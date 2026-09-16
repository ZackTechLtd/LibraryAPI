using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using LibraryAPIApp.Util;

namespace LibraryAPIApp.Tests.Unit;

public class JwtTokenBuilderTests
{
    private static JwtTokenBuilder CreateValidBuilder()
    {
        return new JwtTokenBuilder()
            .AddSecurityKey(JwtSecurityKey.Create("ZackTechSecretKey"))
            .AddSubject("testuser@example.com")
            .AddIssuer("test-issuer")
            .AddAudience("test-audience")
            .AddExpiry(5);
    }

    [Fact]
    public void Build_WithValidArguments_ReturnsWritableJwt()
    {
        var token = CreateValidBuilder().Build();

        Assert.False(string.IsNullOrEmpty(token.Value));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token.Value);

        Assert.Equal("testuser@example.com", jwt.Subject);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.Equal("testuser@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public void Build_WithAddedClaim_IncludesClaimInToken()
    {
        var token = CreateValidBuilder().AddClaim("MembershipId", "111").Build();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);

        Assert.Contains(jwt.Claims, c => c.Type == "MembershipId" && c.Value == "111");
    }

    [Fact]
    public void Build_WithExpiry_RespectsRequestedLifetime()
    {
        var token = CreateValidBuilder().AddExpiry(60).Build();

        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(58));
        Assert.True(token.ValidTo <= DateTime.UtcNow.AddMinutes(61));
    }

    [Fact]
    public void Build_WithoutSecurityKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenBuilder()
                .AddSubject("subject")
                .AddIssuer("issuer")
                .AddAudience("audience")
                .Build());
    }

    [Fact]
    public void Build_WithoutSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenBuilder()
                .AddSecurityKey(JwtSecurityKey.Create("secret"))
                .AddIssuer("issuer")
                .AddAudience("audience")
                .Build());
    }

    [Fact]
    public void Build_WithoutIssuer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenBuilder()
                .AddSecurityKey(JwtSecurityKey.Create("secret"))
                .AddSubject("subject")
                .AddAudience("audience")
                .Build());
    }

    [Fact]
    public void Build_WithoutAudience_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenBuilder()
                .AddSecurityKey(JwtSecurityKey.Create("secret"))
                .AddSubject("subject")
                .AddIssuer("issuer")
                .Build());
    }
}