using System.Security.Claims;
using Common.Models.Api;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers;



[Produces("application/json")]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserWebApiManager _userWebApiManager;

    public UserController(IUserWebApiManager userWebApiManager)
    {
        _userWebApiManager = userWebApiManager;
    }

    /// <summary>
    /// Retrieves information about the current user
    /// </summary>
    /// <returns>The <see cref="UserInfoApiModel"/> model</returns>
    [HttpGet]
    [Route("UserInfo")]
    public UserInfoApiModel GetUserInfo(string username)
    {
        UserInfoApiModel userInfoApiModel = new UserInfoApiModel();

        if (string.IsNullOrEmpty(username))
        {
            return userInfoApiModel;
        }


        //var Name = this.GetCurrentUser();
        //var Id = User.Identity.GetUserId();
        var user = _userWebApiManager.GetUserByUserName(username);
        if (user == null)
        {
            return userInfoApiModel;
        }

        if (userInfoApiModel.Settings == null)
        {
            userInfoApiModel.Settings = new Dictionary<string, string>();
        }


        var roles = _userWebApiManager.GetUserAndRolesByUserName(username, GetCurrentUser());
        Role userDetails = roles.FirstOrDefault(x => x.RoleName == "Administrator") ?? default!;


        userInfoApiModel.IsAdmin = userDetails != null && userDetails.User != null && !string.IsNullOrEmpty(user.UserName);
        userDetails = roles.FirstOrDefault(x => x.RoleName == "Librarian") ?? default!;
        userInfoApiModel.IsLibrarian = userDetails != null && userDetails.User != null && !string.IsNullOrEmpty(user.UserName);

        //Roles that the user belongs to
        userInfoApiModel.RoleList = roles.Where(x => x.User != null).Select(x => x.RoleName);

        var identity = User.Identity as ClaimsIdentity;
        //List<RolesApiModel> roleList = new List<RolesApiModel>();
        List<ClaimApiModel> claimList = new List<ClaimApiModel>();
        if (identity != null)
        {
            foreach (var claim in identity.Claims)
            {
                claimList.Add(new ClaimApiModel
                {
                    ClaimName = claim.Value
                });

            }
        }

        userInfoApiModel.UserClaims = claimList;

        return userInfoApiModel;
    }
}

