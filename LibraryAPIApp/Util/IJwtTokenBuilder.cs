
namespace LibraryAPIApp.Util
{
    using Microsoft.IdentityModel.Tokens;
    using System.Collections.Generic;

    public interface IJwtTokenBuilder
    {
        JwtTokenBuilder AddSecurityKey(SecurityKey securityKey);

        JwtTokenBuilder AddSubject(string subject);

        JwtTokenBuilder AddIssuer(string issuer);

        JwtTokenBuilder AddAudience(string audience);

        JwtTokenBuilder AddClaim(string type, string value);

        JwtTokenBuilder AddClaims(Dictionary<string, string> claims);

        JwtTokenBuilder AddExpiry(int expiryInMinutes);

        JwtToken Build();

    }
}
