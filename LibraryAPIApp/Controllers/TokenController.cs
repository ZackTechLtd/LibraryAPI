

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
    using System.Linq;
    using System.Threading.Tasks;

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

                IJwtTokenBuilder tb;

                if (roles.Count(r => r == "Administrator") > 0)
                {
                    tb = _jwtTokenBuilder
                                .AddSubject(inputModel.Username)
                                .AddClaim("AdministratorId", "")
                                .AddClaim("MembershipId", "111")
                                .AddExpiry(defaultTimeout);
                }
                else
                {
                    tb = _jwtTokenBuilder
                                .AddSubject(inputModel.Username)
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
    }
}