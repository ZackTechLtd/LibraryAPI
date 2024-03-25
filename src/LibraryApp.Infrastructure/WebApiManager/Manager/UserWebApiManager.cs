using System;
using Common.Models;
using Common.Models.Api;
using LibraryApp.Infrastructure.Identity;
using LibraryApp.Infrastructure.Repository;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;

namespace LibraryApp.Infrastructure.WebApiManager.Manager;

public class UserWebApiManager : IUserWebApiManager
{
    /// <summary>
    /// The uServe service repository.
    /// </summary>
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserWebApiManager" /> class.
    /// </summary>
    /// <param name="userRepository">
    /// The User repository
    /// </param>
    public UserWebApiManager(
        IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <summary>
    /// Gets an Id
    /// </summary>
    /// <param name="userName">The UserName of the Identity User</param>
    /// <returns><see cref="ApplicationUser" /></returns>
    public ApplicationUser GetUserByUserName(string userName)
    {
        return this.userRepository.GetUserByUserName(userName);
    }

    public IEnumerable<User> GetUsersPaged(PagedBase filterParameters, string userName, bool showAllUsers, out int searchResultCount)
    {
        return this.userRepository.GetUsersPaged(filterParameters, userName, showAllUsers, out searchResultCount);
    }

    public IEnumerable<Role> GetUserAndRolesByUserName(string userName, string webapiUserName)
    {
        return this.userRepository.GetUserAndRolesByUserName(userName, webapiUserName);
    }

    public IEnumerable<Role> GetUserOwnRolesByUserName(string userName)
    {
        return this.userRepository.GetUserOwnRolesByUserName(userName);
    }

    public IEnumerable<Claim> GetUserAndClaimsByUserName(string userName)
    {
        return this.userRepository.GetUserAndClaimsByUserName(userName);
    }

    public int UpdateUser(User user)
    {
        return this.userRepository.UpdateUser(user);
    }

    public IEnumerable<Role> GetAllRoles()
    {
        return this.userRepository.GetAllRoles();
    }

    public IEnumerable<string> GetGroupedClaims()
    {
        return this.userRepository.GetGroupedClaims();
    }

    public IEnumerable<ClaimWithRole> GetAllClaimsWithRoles()
    {
        return this.userRepository.GetAllClaimsWithRoles();
    }

    public int GetNumberOfCompanyUsersByBranchCode(string branchCode, bool excludeAdministrators = true)
    {
        return this.userRepository.GetNumberOfCompanyUsersByBranchCode(branchCode, excludeAdministrators);
    }

    public int DeleteUser(string userName)
    {
        return this.userRepository.DeleteUser(userName);
    }

    //public int UpdateUserCompany(string username, string companyCode)
    //{
    //    return this.userRepository.UpdateUserCompany(username, companyCode);
    //}

    //public int UpdateUserBranch(string username, string branchCode)
    //{
    //    return this.userRepository.UpdateUserBranch(username, branchCode);
    //}

    public bool IsUserInRole(string roleName, string userName)
    {
        return this.userRepository.IsUserInRole(roleName, userName);
    }
}