
namespace DataAccess.WebApiRepository.Interfaces
{
    using Common.Models;
    using Common.Models.Api;
    using DataAccess.IdentityModels;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IUserRepository
    {
        ApplicationUser GetUserByUserName(string userName);

        IEnumerable<User> GetUsersPaged(PagedBase filterParameters, string userName, bool showAllUsers, out int searchResultCount);

        IEnumerable<Role> GetUserAndRolesByUserName(string userName, string webapiUserName);
        IEnumerable<Role> GetUserOwnRolesByUserName(string userName);
        IEnumerable<Claim> GetUserAndClaimsByUserName(string userName);

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
