using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LibraryApp.Api.UnitTests;

public abstract class BaseTests
{
    public HttpRequestMessage AddAuthorisation(string role, ControllerBase controller)
    {
        var token = CreateBearerToken("role");

        // Set up HttpClient with Bearer token
        //var client = new HttpClient();
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "your_api_endpoint_here");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        // Set Authorization header in controller context
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + token;

        return request;

    }

    public static string CreateBearerToken(string role)
    {
        var BearerTokenSigningKey = "StubPrivateKey123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("role", role) };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(BearerTokenSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: user.Claims,
            signingCredentials: creds,
            expires: DateTime.UtcNow.AddMinutes(5)
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

