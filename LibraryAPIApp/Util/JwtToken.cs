
namespace LibraryAPIApp.Util
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;

    public sealed class JwtToken : IJwtToken
    {
        private JwtSecurityToken token;

        internal JwtToken(JwtSecurityToken token)
        {
            this.token = token;
        }

        public DateTime ValidTo => token.ValidTo;
        public string Value => new JwtSecurityTokenHandler().WriteToken(this.token);
    }
}
