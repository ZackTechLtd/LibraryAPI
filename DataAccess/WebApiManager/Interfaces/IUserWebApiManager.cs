
namespace DataAccess.WebApiManager.Interfaces
{
    using Common.Models;
    using Common.Models.Api;
    using DataAccess.IdentityModels;
    using System.Collections.Generic;


    public interface IUserWebApiManager
    {
        /// <summary>
        /// Get an Identity User (and additional information) by the UserName
        /// </summary>
        /// <param name="userName">
        /// The Name of the User
        /// </param>
        /// <returns>The <see cref="ApplicationUser"/></returns>
        ApplicationUser GetUserByUserName(string userName);

        IEnumerable<User> GetUsersPaged(PagedBase filterParameters, string userName, bool showAllUsers, out int searchResultCount);

        IEnumerable<Role> GetUserAndRolesByUserName(string userName, string webapiUserName);

        IEnumerable<Claim> GetUserAndClaimsByUserName(string userName);

        IEnumerable<Role> GetUserOwnRolesByUserName(string userName);

        IEnumerable<Role> GetAllRoles();

        IEnumerable<string> GetGroupedClaims();

        IEnumerable<ClaimWithRole> GetAllClaimsWithRoles();
        int GetNumberOfCompanyUsersByBranchCode(string branchCode, bool excludeAdministrators = true);
        int UpdateUser(User user);
        int DeleteUser(string userName);

        //int UpdateUserCompany(string username, string companyCode);

        //int UpdateUserBranch(string username, string branchCode);

        bool IsUserInRole(string roleName, string userName);
    }
}
