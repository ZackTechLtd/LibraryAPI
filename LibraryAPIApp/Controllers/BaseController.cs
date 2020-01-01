

namespace LibraryAPIApp.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IdentityModel.Tokens.Jwt;

    public class BaseController : ControllerBase
    {
        protected JwtSecurityToken GetUserToken()
        {
            String headerValue = Request.Headers["Authorization"];
            if (headerValue != null)
            {
                JwtSecurityTokenHandler jwt_tokenhandler = new JwtSecurityTokenHandler();
                string[] tokenarray;
                string strtoken;
                if (headerValue.Contains("Bearer"))
                {
                    tokenarray = headerValue.Substring("Bearer ".Length).Split(new char[] { '=' }, 2);
                    strtoken = tokenarray[0];
                }
                else
                {
                    tokenarray = headerValue.Substring("WRAP ".Length).Split(new char[] { '=' }, 2);
                    strtoken = tokenarray[1].Substring(1, tokenarray[1].Length - 2);
                }

                return new JwtSecurityToken(jwtEncodedString: strtoken);

            }

            return null;
        }
        protected string GetCurrentUser()
        {
            JwtSecurityToken token = GetUserToken();
            if (token != null)
            {
                return token.Subject;
            }

            return null;
        }
    }
}
