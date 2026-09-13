
namespace LibraryAPIApp.Util
{
    using Microsoft.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Text;

    public static class JwtSecurityKey
    {
        public static SymmetricSecurityKey Create(string secret)
        {
            // Derive a 256-bit key from any-length secret so HS256 validation is satisfied.
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            return new SymmetricSecurityKey(key);
        }
    }
}