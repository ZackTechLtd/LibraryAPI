

namespace LibraryAPIApp.Controllers
{
    using Common.Configuration;
    using Common.Models.Api;
    using DataAccess.IdentityModels;
    using LibraryAPIApp.Util;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using System.Linq;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;

    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TokenController : BaseController
    {
        private readonly IJwtTokenBuilder _jwtTokenBuilder;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<ApiConfiguration> _apiConfiguration;

        public TokenController(IOptions<ApiConfiguration> apiConfiguration,
            IJwtTokenBuilder jwtTokenBuilder,
            UserManager<ApplicationUser> userManager
        )
        {
            _jwtTokenBuilder = jwtTokenBuilder;
            _userManager = userManager;
            _apiConfiguration = apiConfiguration;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApiUser inputModel)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(inputModel.Username);
                if (user == null)
                {
                    return Unauthorized();
                }

                if (!int.TryParse(_apiConfiguration.Value.DefaultTimeout, out int defaultTimeout))
                {
                    defaultTimeout = 60;
                }

                if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                {
                    defaultTimeout += 60;
                }

                if (!await _userManager.CheckPasswordAsync(user, inputModel.Password))
                {
                    return Unauthorized();
                }

                var userclaims = await _userManager.GetClaimsAsync(user);

                var roles = await _userManager.GetRolesAsync(user);

                IJwtTokenBuilder tb = null;

                if (roles.Where(x => x == "Administrator").Count() > 0)
                {
                    //return Ok(GenerateJWT(user));
                    
                    tb = _jwtTokenBuilder.AddSecurityKey(JwtSecurityKey.Create("ZackTechSecretKey"))
                                .AddSubject(inputModel.Username)
                                    .AddIssuer("ZackTechSecurityBearer")
                                    .AddAudience("ZackTechSecurityBearer")
                                    //.AddClaim(JwtRegisteredClaimNames.Sub, user.UserName)
                                    .AddClaim("AdministratorId", "")
                                    //.AddClaim("SeniorLibrarianId", "333")
                                    .AddClaim("MembershipId", "111")
                                    .AddExpiry(defaultTimeout);
                                    
                }
                else
                {
                    tb = _jwtTokenBuilder.AddSecurityKey(JwtSecurityKey.Create("ZackTechSecretKey"))
                                .AddSubject(inputModel.Username)
                                    .AddIssuer("ZackTechSecurityBearer")
                                    .AddAudience("ZackTechSecurityBearer")
                                    //.AddClaim(JwtRegisteredClaimNames.Sub, user.UserName)
                                    .AddClaim("MembershipId", "111")
                                    .AddExpiry(defaultTimeout);
                }

                foreach (var claim in userclaims)
                {
                    tb.AddClaim(claim.Value, claim.Value);
                }

                var token = tb.Build();

                return Ok(token.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError("Login Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> LogOff()
        {
            var user = await _userManager.FindByNameAsync(GetCurrentUser());
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok();

        }
        /*
        string GenerateJWT(ApplicationUser userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ZackTech-secret-key"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new System.Security.Claims.Claim("AdministratorId", ""),
                new System.Security.Claims.Claim("MembershipId",""),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: "ZackTechSecurityBearer",
                audience: "ZackTechSecurityBearer",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        */
    }
}