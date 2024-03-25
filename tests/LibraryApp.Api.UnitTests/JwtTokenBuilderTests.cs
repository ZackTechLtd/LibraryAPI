using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;


namespace LibraryApp.Api.UnitTests;

public class JwtTokenBuilderTests
{
    [Fact]
    public void Build_Should_Create_Valid_JwtToken()
    {
        // Arrange
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("StubPrivateKey123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
        var subject = "test_subject";
        var issuer = "test_issuer";
        var audience = "test_audience";
        var claims = new Dictionary<string, string>
            {
                { "claim1", "value1" },
                { "claim2", "value2" }
            };
        var expiryInMinutes = 10;

        var builder = new LibraryApp.Core.Util.JwtTokenBuilder()
            .AddSecurityKey(securityKey)
            .AddSubject(subject)
            .AddIssuer(issuer)
            .AddAudience(audience)
            .AddClaims(claims)
            .AddExpiry(expiryInMinutes);

        // Act
        var jwtToken = builder.Build();

        // Assert
        jwtToken.Should().NotBeNull();
        jwtToken.Value.Should().NotBeNullOrWhiteSpace();

        // Validate token
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(jwtToken.Value, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = securityKey
        }, out var validatedToken);

        var jwtSecurityToken = validatedToken as JwtSecurityToken;
        jwtSecurityToken.Should().NotBeNull();

        jwtSecurityToken!.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == subject);
        jwtSecurityToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Jti);
     
    }

    [Fact]
    public void Build_Should_Throw_Exception_When_Arguments_Are_Missing()
    {
        // Arrange
        var builder = new LibraryApp.Core.Util.JwtTokenBuilder();

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().BeOneOf("Security Key", "Subject", "Issuer", "Audience");
    }
}
