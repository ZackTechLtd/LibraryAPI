using System;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryApp.Api.Controllers;

public class BaseController : ControllerBase
{
    protected JwtSecurityToken GetUserToken()
    {
        string? headerValue = Request.Headers["Authorization"];
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

        return default!;
    }
    protected string GetCurrentUser()
    {
        JwtSecurityToken token = GetUserToken();
        if (token != null)
        {
            return token.Subject;
        }

        return default!;
    }
}

