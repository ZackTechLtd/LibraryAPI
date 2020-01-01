using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Models.Api;
using DataAccess.WebApiManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LibraryAPIApp.Controllers
{
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
            Role userDetails = roles.FirstOrDefault(x => x.RoleName == "Administrator");


            userInfoApiModel.IsAdmin = userDetails != null && userDetails.User != null && !string.IsNullOrEmpty(user.UserName);
            userDetails = roles.FirstOrDefault(x => x.RoleName == "Librarian");
            userInfoApiModel.IsLibrarian = userDetails != null && userDetails.User != null && !string.IsNullOrEmpty(user.UserName);

            //Roles that the user belongs to
            userInfoApiModel.RoleList = roles.Where(x => x.User != null).Select(x => x.RoleName);

            var identity = User.Identity as ClaimsIdentity;
            //List<RolesApiModel> roleList = new List<RolesApiModel>();
            List<ClaimApiModel> claimList = new List<ClaimApiModel>();
            foreach (var claim in identity.Claims)
            {
                claimList.Add(new ClaimApiModel
                {
                    ClaimName = claim.Value
                });

            }

            userInfoApiModel.UserClaims = claimList;

            return userInfoApiModel;
        }
    }
}
