using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Common.Configuration;
using Common.Models;
using Common.Models.Api;
using Common.Util;
using Dapper;
using LibraryApp.Infrastructure.Helper;
using LibraryApp.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LibraryApp.Infrastructure.Repository;

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

public class UserRepository : RepositoryBase, IUserRepository
{
    protected readonly IRandomKeyGenerator _randomKeyGenerator;
    protected readonly IOptions<ApiConfiguration> _apiConfiguration;
    private static readonly Object _obj = new Object();

    /// <summary>
    /// Initializes a new instance of the<see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="appConfig">
    /// The app config.
    /// </param>
    public UserRepository(IConfiguration config, IOptions<ApiConfiguration> apiConfiguration, IRandomKeyGenerator randomKeyGenerator) : base(config, apiConfiguration)
    {
        _apiConfiguration = apiConfiguration;
        _randomKeyGenerator = randomKeyGenerator;
    }

    /// <summary>
    /// Get Number of assigned users to a company
    /// </summary>
    /// <param name="branchCode">
    /// <param name="excludeAdministrators">
    /// The number of users
    /// </param>
    /// <returns>The <see cref="int"/></returns>
    public virtual int GetNumberOfCompanyUsersByBranchCode(string branchCode, bool excludeAdministrators = true)
    {
        string aspNetUsers = " [Identity].AspNetUsers ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUsers = " AspNetUsers ";
        }

        string sql = string.Empty;

        if (excludeAdministrators)
        {
            string aspNetUserRoles = " [Identity].AspNetUserRoles ";
            if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
            {
                aspNetUserRoles = " AspNetUserRoles ";
            }

            string aspNetRoles = " [Identity].AspNetRoles ";
            if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
            {
                aspNetRoles = " AspNetRoles ";
            }


            sql = string.Format(@"select Count(*) from {0} U
                                INNER JOIN Branch B ON U.CompanyId = B.CompanyId
                                INNER JOIN {1} UR ON U.Id = UR.UserId
                                INNER JOIN {2} R ON UR.RoleId = R.Id
                                WHERE R.Name <> 'Administrator' AND  B.BranchCode = @branchCode;", aspNetUsers, aspNetUserRoles, aspNetRoles);
        }
        else
        {
            sql = string.Format(@"select Count(*) from {0} U
                                INNER JOIN Branch B ON U.CompanyId = B.CompanyId
                                WHERE B.BranchCode = @userName;", aspNetUsers);
        }


        var parameters = new { branchCode };

        int result = 0;

        using (var connection = this.OpenConnection())
        {
            result = connection.Query<int>(
            sql,
            param: parameters,
            commandType: CommandType.Text)
            .SingleOrDefault();
        }

        return result;
    }

#pragma warning disable CS8600

    /// <summary>
    /// Get an Identity User (and additional information) by the UserName
    /// </summary>
    /// <param name="userName">
    /// The Name of the User
    /// </param>
    /// <returns>The <see cref="ApplicationUser"/></returns>
    public virtual ApplicationUser GetUserByUserName(string userName)
    {
        ApplicationUser user = default!;

        string aspNetUsers = " [Identity].AspNetUsers ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUsers = " AspNetUsers ";
        }

        string sql = string.Format(@"SELECT U.Email, U.UserName, U.LastPasswordChangedDate, C.CompanyId, C.CompanyCode, C.CompanyName, B.BranchCode, B.BranchName
	                    FROM {0} U
		                    LEFT OUTER JOIN Company C ON U.CompanyId = C.CompanyId
		                    LEFT OUTER JOIN Branch B ON U.BranchId = B.BranchId 
                            WHERE U.UserName = @userName;", aspNetUsers);

        var parameters = new { userName };

        using (var connection = this.OpenConnection())
        {
            user = connection.Query<ApplicationUser>(
            sql,
            param: parameters,
            commandType: CommandType.Text)
            .FirstOrDefault();
        }

        return user ?? default!;
    }



    public virtual bool IsUserInRole(string roleName, string userName)
    {
        string userId = default!;

        string aspNetUsers = " [Identity].AspNetUsers ";
        string aspNetUserRoles = " [Identity].AspNetUserRoles ";
        string aspNetRoles = " [Identity].AspNetRoles ";


        var parameters = new { roleName, userName };
        string sql = $"SELECT U.Id FROM {aspNetUsers} U INNER JOIN {aspNetUserRoles} UR ON U.Id = UR.UserId INNER JOIN {aspNetRoles} R ON UR.RoleId = R.Id WHERE U.UserName = @userName AND R.Name = @roleName;";

        using (var connection = this.OpenConnection())
        {
            userId = connection.Query<string>(
            sql,
            param: parameters,
            commandType: CommandType.Text).FirstOrDefault();
        }

        return !string.IsNullOrEmpty(userId);
    }

#pragma warning restore CS8600

    public virtual IEnumerable<User> GetUsersPaged(PagedBase filterParameters, string userName, bool showAllUsers, out int searchResultCount)
    {
        searchResultCount = 0;

        IEnumerable<UserListItem> userListitems = default!;

        var pageOffset = (filterParameters.PageSize * (filterParameters.PageNum - 1));

        string aspNetUsers = " [Identity].AspNetUsers ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUsers = " AspNetUsers ";
        }

        int companyId = 0;
        using (var connection = this.OpenConnection())
        {
            companyId = connection.Query<int>(
                string.Format(@"SELECT C.CompanyId FROM Company C
                    INNER JOIN {0} U ON U.UserName = @userName AND C.CompanyId = U.CompanyId", aspNetUsers), new { userName }
                ).SingleOrDefault();
        }

        var parameters = new
        {
            userName,
            companyId,
            searchText = "%" + filterParameters.SearchText + "%",
            pageOffset,
            pageSize = filterParameters.PageSize,
            orderBy = filterParameters.OrderBy,
            sortOrder = filterParameters.SortOrder
        };


        string sql = default!;
        if (IsUserInRole("Administrator", userName))
        {
            if (showAllUsers)
            {
                sql = string.Format(@"SELECT U1.UserName, U1.Email As 'EmailAddress', 
                                    (SELECT COUNT(*) FROM {0} U2
                                    INNER JOIN Company C1 ON U2.CompanyId = C1.CompanyId
                                    LEFT OUTER JOIN Branch B ON U2.BranchId = B.BranchId) AS 'TotalRows', B.BranchCode, B.BranchName
                                    FROM {1} U1
                                    INNER JOIN Company C ON U1.CompanyId = C.CompanyId
                                    LEFT OUTER JOIN Branch B ON U1.BranchId = B.BranchId
                            WHERE
                                U1.UserName LIKE @searchText 
                                AND
                                (
                                    U1.Email LIKE @searchText    
			                        OR B.BranchName LIKE @searchText
                                )
                            order by
                                CASE WHEN @orderBy = 0 AND @sortOrder = 0 THEN U1.UserName END ASC,
		                         CASE WHEN @orderBy = 0 AND @sortOrder = 1 THEN U1.UserName END DESC,
		                         CASE WHEN @orderBy = 1 AND @sortOrder = 0 THEN U1.Email END ASC,
		                         CASE WHEN @orderBy = 1 AND @sortOrder = 1 THEN U1.Email END DESC,		
		                         CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN B.BranchName END ASC,
		                         CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN B.BranchName END DESC ", aspNetUsers, aspNetUsers);
            }
            else
            {
                sql = string.Format(@"SELECT U1.UserName, U1.Email As 'EmailAddress', 
                                    (SELECT COUNT(*) FROM {0} U2
                                    INNER JOIN Company C1 ON U2.CompanyId = C1.CompanyId
                                    LEFT OUTER JOIN Branch B ON U2.BranchId = B.BranchId
                                    WHERE U2.CompanyId = @companyId) AS 'TotalRows', B.BranchCode, B.BranchName
                                    FROM {1} U1
                                    INNER JOIN Company C ON U1.CompanyId = C.CompanyId
                                    LEFT OUTER JOIN Branch B ON U1.BranchId = B.BranchId
            WHERE
                U1.CompanyId = @companyId AND U1.UserName LIKE @searchText 
                AND
                (
                    U1.Email LIKE @searchText    
			        OR B.BranchName LIKE @searchText
                )
            order by
                CASE WHEN @orderBy = 0 AND @sortOrder = 0 THEN U1.UserName END ASC,
		         CASE WHEN @orderBy = 0 AND @sortOrder = 1 THEN U1.UserName END DESC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 0 THEN U1.Email END ASC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 1 THEN U1.Email END DESC,		
		         CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN B.BranchName END ASC,
		         CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN B.BranchName END DESC ", aspNetUsers, aspNetUsers);

            }

        }
        else
        {
            string aspNetUserRoles = " [Identity].AspNetUserRoles ";


            string aspNetRoles = " [Identity].AspNetRoles ";


            sql = string.Format(@"SELECT U1.UserName, U1.Email 'EmailAddress',
                (SELECT COUNT(*) FROM {0} U2
                LEFT OUTER JOIN Branch B ON U2.BranchId = B.BranchId
                INNER JOIN {1} UR ON UR.UserId = U2.Id
                INNER JOIN {2} R ON R.Id = UR.RoleId
                WHERE U2.CompanyId = @companyId AND R.Name <> 'Administrator') AS 'TotalRows',  B.BranchCode, B.BranchName
                FROM {3} U1
                LEFT OUTER JOIN Branch B ON U1.BranchId = B.BranchId
                INNER JOIN {4} UR ON UR.UserId = U1.Id
                INNER JOIN {5} R ON R.Id = UR.RoleId
            WHERE U1.CompanyId = @companyId AND R.Name <> 'Administrator'
                AND
                (
                    U1.UserName LIKE @searchText 
                    OR U1.Email LIKE @searchText
			        OR B.BranchName LIKE @searchText
                )
            order by
                CASE WHEN @orderBy = 0 AND @sortOrder = 0 THEN U1.UserName END ASC,
		         CASE WHEN @orderBy = 0 AND @sortOrder = 1 THEN U1.UserName END DESC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 0 THEN U1.Email END ASC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 1 THEN U1.Email END DESC,		
		         CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN B.BranchName END ASC,
		         CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN B.BranchName END DESC ", aspNetUsers, aspNetUserRoles, aspNetRoles, aspNetUsers, aspNetUserRoles, aspNetRoles);
        }




        sql += @"OFFSET @pageOffset ROWS
                    FETCH NEXT @pageSize ROWS ONLY";



        using (var connection = this.OpenConnection())
        {
            userListitems = connection.Query<UserListItem>(
            sql,
            param: parameters,
            commandType: CommandType.Text);
        }

        if (userListitems != null && userListitems.Count() > 0)
        {
            searchResultCount = userListitems.First().TotalRows;
        }

        IEnumerable<User> users = userListitems!.Select(x => new User
        {
            UserName = x.UserName,
            EmailAddress = x.EmailAddress,
            //Branch = x.Branch

        }).ToList();

        return users;
    }

    /// <summary>
    /// Get  User And Roles By UserName
    /// </summary>
    /// <returns>The <see cref="userName"/> </returns>
    public virtual IEnumerable<Role> GetUserAndRolesByUserName(string userName, string webapiUserName)
    {
        lock (_obj)
        {

            var parameters = new { userName };

            IEnumerable<Role> roles = default!;

            string aspNetUsers = " [Identity].AspNetUsers ";
            if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
            {
                aspNetUsers = " AspNetUsers ";
            }

            string aspNetUserRoles = " [Identity].AspNetUserRoles ";
            if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
            {
                aspNetUserRoles = " AspNetUserRoles ";
            }

            string aspNetRoles = " [Identity].AspNetRoles ";
            if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
            {
                aspNetRoles = " AspNetRoles ";
            }

            //TODO not sure how @webapiUserName
            //OR U.UserName = @webapiUserName

            string sql = string.Format(@"SELECT  R.Id As 'RoleId', R.Name As 'RoleName', Null As 'UserName', null As 'EmailAddress', null As 'PhoneNumber', Null As 'BranchCode', null As 'LockoutEnabled', null As 'LockoutEnd'
            FROM {0} R
            UNION
            SELECT R.Id 'RoleId', R.Name 'RoleName', U.UserName, U.Email 'EmailAddress', U.PhoneNumber, B.BranchCode, U.LockoutEnabled, U.LockoutEnd
            FROM {1} U
            LEFT OUTER JOIN {2} UR ON UR.UserId = U.Id
            LEFT OUTER JOIN {3} R ON R.Id = UR.RoleId
            LEFT OUTER JOIN Branch B ON U.BranchId = B.BranchId
             WHERE U.UserName = @userName 
            Order By UserName DESC, RoleName;", aspNetRoles, aspNetUsers, aspNetUserRoles, aspNetRoles);

            using (var connection = this.OpenConnection())
            {
                roles = connection.Query<Role, User, Role>(
                sql,
                (cs, c) =>
                {
                    cs.User = c;
                    return cs;
                },
                splitOn: "UserName",
                param: parameters,
                commandType: CommandType.Text);
            }

            if (roles.FirstOrDefault(x => x.User != null) == null)
            {
                var param = new { userName = webapiUserName };
                using (var connection = this.OpenConnection())
                {
                    roles = connection.Query<Role, User, Role>(
                    sql,
                    (cs, c) =>
                    {
                        cs.User = c;
                        return cs;
                    },
                    splitOn: "UserName",
                    param: param,
                    commandType: CommandType.Text);
                }
            }

            Dictionary<string, Role> dicItems = new Dictionary<string, Role>();
            foreach (Role role in roles)
            {
                if (string.IsNullOrEmpty(role.RoleName))
                {
                    if (!dicItems.ContainsKey(""))
                    {
                        dicItems[""] = role;
                    }
                }
                else
                {
                    if (!dicItems.ContainsKey(role.RoleName))
                    {
                        dicItems[role.RoleName] = role;
                    }
                }
            }

            roles = dicItems.Values;
            return roles;
        }
    }

    /// <summary>
    /// Get  User Own Roles By UserName
    /// </summary>
    /// <returns>The <see cref="userName"/> </returns>
    public virtual IEnumerable<Role> GetUserOwnRolesByUserName(string userName)
    {
        lock (_obj)
        {

            var parameters = new { userName };

            IEnumerable<Role> roles = default!;

            string aspNetUsers = " [Identity].AspNetUsers ";


            string aspNetUserRoles = " [Identity].AspNetUserRoles ";


            string aspNetRoles = " [Identity].AspNetRoles ";


            string sql = string.Format(@"SELECT  R.Id As 'RoleId', R.Name As 'RoleName', U.UserName As 'UserName', U.Email As 'EmailAddress', U.PhoneNumber As 'PhoneNumber', U.LockoutEnabled As 'LockoutEnabled', U.LockoutEnd As 'LockoutEnd', B.BranchCode As 'BranchCode'
                        FROM {0} R
			            INNER JOIN {1} UR ON R.Id = UR.RoleId
			            INNER JOIN {2} U ON UR.UserId = U.Id
			            LEFT OUTER JOIN Branch B ON U.BranchId = B.BranchId
			            WHERE U.UserName = @userName 
            Order By UserName DESC, RoleName;", aspNetRoles, aspNetUserRoles, aspNetUsers);

            using (var connection = this.OpenConnection())
            {
                roles = connection.Query<Role, User, Role>(
                sql,
                (cs, c) =>
                {
                    cs.User = c;
                    return cs;
                },
                splitOn: "UserName",
                param: parameters,
                commandType: CommandType.Text);
            }

            return roles;
        }
    }

    /// <summary>
    /// Get User And Claims By UserName
    /// </summary>
    /// <returns>The <see cref="userName"/> </returns>
    public virtual IEnumerable<Claim> GetUserAndClaimsByUserName(string userName)
    {

        string aspNetUsers = " [Identity].AspNetUsers ";


        string aspNetUserClaims = " [Identity].AspNetUserClaims ";


        var parameters = new { userName };

        IEnumerable<Claim> roles = default!;

        string sql = string.Format(@"SELECT R.Id As 'ClaimId', R.ClaimValue As 'ClaimValue', U.UserName, U.Email As 'EmailAddress', U.PhoneNumber,B.BranchCode,U.LockoutEnabled, U.LockoutEnd 
                            FROM {0} U
                            LEFT OUTER JOIN {1} R ON R.UserId = U.Id
                            LEFT OUTER JOIN Branch B ON U.BranchId = B.BranchId
                            WHERE U.UserName = @userName;", aspNetUsers, aspNetUserClaims);


        using (var connection = this.OpenConnection())
        {
            roles = connection.Query<Claim, User, Claim>(
            sql,
            (cs, c) =>
            {
                cs.User = c;
                return cs;
            },
            splitOn: "UserName",
            param: parameters,
            commandType: CommandType.Text);
        }

        return roles;
    }

    /// <summary>
    /// Get all Roles
    /// </summary>
    /// <returns>The <see cref="role"/> </returns>
    public virtual IEnumerable<Role> GetAllRoles()
    {
        IEnumerable<Role> roles = default!;

        string aspNetRoles = " [Identity].AspNetRoles ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetRoles = " AspNetRoles ";
        }

        string sql = $"SELECT Id As 'RoleId',Name As 'RoleName' FROM  {aspNetRoles}";
        using (var connection = this.OpenConnection())
        {
            roles = connection.Query<Role>(
            sql,
            commandType: CommandType.Text);
        }

        return roles;
    }

    /// <summary>
    /// Get All Claims With Roles
    /// </summary>
    /// <returns>The <see cref="role"/> </returns>
    public virtual IEnumerable<ClaimWithRole> GetAllClaimsWithRoles()
    {
        IEnumerable<ClaimWithRole> result = default!;

        string aspNetRoleClaims = " [Identity].AspNetRoleClaims ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetRoleClaims = " AspNetRoleClaims ";
        }

        string aspNetRoles = " [Identity].AspNetRoles ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetRoles = " AspNetRoles ";
        }

        string sql = $"SELECT RC.Id AS ClaimId, RC.ClaimValue, R.Name FROM {aspNetRoleClaims} RC LEFT OUTER JOIN {aspNetRoles} R ON RC.RoleId = R.Id";
        using (var connection = this.OpenConnection())
        {
            result = connection.Query<ClaimWithRole>(
            sql,
            commandType: CommandType.Text);
        }

        return result;
    }

    public virtual IEnumerable<string> GetGroupedClaims()
    {
        IEnumerable<string> result = default!;

        string aspNetRoleClaims = " [Identity].AspNetRoleClaims ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetRoleClaims = " AspNetRoleClaims ";
        }

        string aspNetUserClaims = " [Identity].AspNetUserClaims ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUserClaims = " AspNetUserClaims ";
        }

        string sql = $"SELECT ClaimValue FROM {aspNetRoleClaims} Group By ClaimValue UNION SELECT ClaimValue FROM {aspNetUserClaims} Group By ClaimValue";
        using (var connection = this.OpenConnection())
        {
            result = connection.Query<string>(
            sql,
            commandType: CommandType.Text);
        }

        return result;
    }


#pragma warning disable CS8602, CS8600
    /// <summary>
    /// Gets the string values.
    /// </summary>
    /// <param name="navigator">The navigator.</param>
    /// <param name="xpath">The xpath.</param>
    /// <returns></returns>
    protected List<string> GetStringValues(XPathNavigator navigator, string xpath)
    {
        List<string> items = new List<string>();
        if (!string.IsNullOrEmpty(xpath))
        {
            XPathNodeIterator bookNodesIterator = navigator.Select(xpath);
            if (bookNodesIterator != null)
            {
                while (bookNodesIterator.MoveNext())
                    items.Add(string.Format("{0}", bookNodesIterator?.Current?.Value));
            }
        }
        return items;
    }


    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public virtual int UpdateUser(User user)
    {
        XDocument doc = XDocument.Parse(user.XmlUserPermissionIds);
        XPathNavigator navigator = doc.CreateNavigator();
        List<string> roleIds = GetStringValues(navigator, "//Role/RoleId");

        string aspNetUsers = " [Identity].AspNetUsers ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUsers = " AspNetUsers ";
        }

        string aspNetUserClaims = " [Identity].AspNetUserClaims ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUserClaims = " AspNetUserClaims ";
        }

        string aspNetUserRoles = " [Identity].AspNetUserRoles ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUserRoles = " AspNetUserRoles ";
        }

        string aspNetRoleClaims = "[Identity].AspNetRoleClaims";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetRoleClaims = " AspNetRoleClaims ";
        }

        string updatesql = string.Format(@"UPDATE {0}
			SET
				BranchId = (SELECT BranchId FROM Branch WHERE BranchCode = @branchCode),
				Email = @emailAddress,
                NormalizedEmail = @normalizedEmail,
				PhoneNumber = @phoneNumber,
				LockoutEnabled = @lockoutEnabled,
				LockoutEnd = @lockoutEndDateUtc,
				CompanyId = (SELECT CompanyId FROM Branch WHERE BranchCode = @branchCode) 
			FROM {1} U
			WHERE U.UserName = @username;", aspNetUsers, aspNetUsers);

        string insertclaimssql = string.Format(@"INSERT INTO {0} (ClaimType,ClaimValue,UserId)
			                            SELECT RC.ClaimType, RC.ClaimValue, @userId
			                            FROM {1} RC
			                            INNER JOIN {2} UR ON RC.RoleId = UR.RoleId 
			                            INNER JOIN {3} U ON UR.UserId = U.Id
				                            where UserId = @userId", aspNetUserClaims, aspNetRoleClaims, aspNetUserRoles, aspNetUsers);

        var parameters = new DynamicParameters();
        parameters.Add(name: "username", value: user.UserName);
        parameters.Add(name: "emailAddress", value: user.EmailAddress);
        parameters.Add(name: "normalizedEmail", value: !string.IsNullOrEmpty(user.EmailAddress) ? user.EmailAddress.ToUpper() : null);
        parameters.Add(name: "phoneNumber", value: user.PhoneNumber);
        parameters.Add(name: "lockoutEnabled", value: user.LockoutEnabled);
        parameters.Add(name: "lockoutEndDateUtc", value: user.LockoutEndDateUtc);
        parameters.Add(name: "branchCode", value: user.BranchCode);
        //parameters.Add(name: "modifiedBy", value: user.ModifiedBy);
        //parameters.Add(name: "modifiedDate", value: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        int rowsAffected = 0;
        using (var connection = this.OpenConnection())
        {

            using (var transaction = BeginSqlTransaction(connection))
            {
                try
                {
                    var param = new { userName = user.UserName };

                    string sql = $"SELECT Id FROM {aspNetUsers} WHERE UserName = @userName;";

                    //Get UserId
                    string userId = connection.Query<string>(
                        sql,
                        param: param,
                        commandType: CommandType.Text,
                        transaction: transaction).SingleOrDefault();

                    //Does Update for user
                    rowsAffected = connection.Execute(
                    updatesql,
                    param: parameters,
                    commandType: CommandType.Text,
                    transaction: transaction);

                    //Deletes all user roles
                    connection.Execute($"DELETE FROM {aspNetUserClaims} WHERE UserId = @userId;", new { userId }, transaction: transaction);
                    connection.Execute($"DELETE FROM {aspNetUserRoles} WHERE UserId = @userId;", new { userId }, transaction: transaction);

                    //Add new user roles
                    foreach (string roleid in roleIds)
                    {
                        sql = $"INSERT INTO  {aspNetUserRoles}  ([UserId],[RoleId]) VALUES ('{userId}','{roleid}');";
                        connection.Execute(
                        sql,
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: transaction);
                    }

                    var insertparam = new { userId };
                    connection.Execute(
                    insertclaimssql,
                    param: insertparam,
                    commandType: CommandType.Text,
                    transaction: transaction);

                    CommitSqlTransaction(transaction);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    RollbackSqlTransaction(transaction, "UpdateUser", new Exception());
                    rowsAffected = 0;
                    return rowsAffected;
                }


            }

        }

        if (rowsAffected > 0)
        {
            AddOrUpdateLastUserCompanyAndBranch(user.UserName);
        }

        return rowsAffected;
    }

#pragma warning restore CS8602, CS8600

    public int UpdateUserOld(User user)
    {

        const string SpName = "[Config].[UpdateUser]";

        var parameters = new DynamicParameters();
        parameters.Add(name: "username", value: user.UserName);
        parameters.Add(name: "emailAddress", value: user.EmailAddress);
        parameters.Add(name: "phoneNumber", value: user.PhoneNumber);
        parameters.Add(name: "lockoutEnabled", value: user.LockoutEnabled);
        parameters.Add(name: "lockoutEndDateUtc", value: user.LockoutEndDateUtc);
        parameters.Add(name: "branchCode", value: user.BranchCode);
        if (!string.IsNullOrEmpty(user.XmlUserPermissionIds))
            parameters.Add(name: "@@userRolesXml", value: user.XmlUserPermissionIds);
        else
            parameters.Add(name: "@@userRolesXml", value: null);
        parameters.Add(name: "modifiedBy", value: user.ModifiedBy);
        parameters.Add(name: "returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        using (var connection = this.OpenConnection())
        {
            connection.Execute(
            SpName,
            param: parameters,
            commandType: CommandType.Text);
        }

        return parameters.Get<int>("returnValue");
    }

    //protected Company GetCompanyByCompanyCode(string companyCode)
    //{
    //    Company company = null;
    //    var parameters = new { companyCode };
    //    using (var connection = this.OpenConnection())
    //    {
    //        company = connection.Query<Company, IdentityModels.Branch, Company>(
    //            "SELECT * from Company C LEFT OUTER JOIN Branch B ON C.CompanyId = B.CompanyId WHERE C.CompanyCode = @companyCode",
    //            (c, b) =>
    //            {
    //                c.Branches = new List<IdentityModels.Branch>();
    //                c.Branches.Add(b);
    //                return c;
    //            },
    //            splitOn: "BranchId",
    //            param: parameters,
    //            commandType: CommandType.Text).FirstOrDefault();
    //    }

    //    return company;
    //}

    //public virtual int UpdateUserCompany(string username, string companyCode)
    //{


    //    string aspNetUsers = " [Identity].AspNetUsers ";
    //    if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
    //    {
    //        aspNetUsers = " AspNetUsers ";
    //    }

    //    string sql = $"UPDATE {aspNetUsers} SET CompanyId = @companyId, BranchId = @branchId WHERE UserName = @username";

    //    var parameters = new DynamicParameters();
    //    parameters.Add(name: "username", value: username);

    //    (int? companyId, int? branchId) = GetUsersLastCompanyIdAndBranchIdByCompanyCode(username, companyCode);

    //    if (companyId != null && companyId > 0 && branchId != null && branchId > 0)
    //    {
    //        parameters.Add(name: "companyId", value: companyId);
    //        parameters.Add(name: "branchId", value: branchId);
    //    }
    //    else
    //    {
    //        Company company = GetCompanyByCompanyCode(companyCode);
    //        if (company == null)
    //            return 0;

    //        branchId = null;
    //        if (company.Branches != null) // && company.Branches.Count() > 0)
    //        {
    //            var branch = company.Branches.FirstOrDefault();
    //            if (branch != null)
    //            {
    //                branchId = branch.BranchId;
    //            }

    //        }

    //        parameters.Add(name: "companyId", value: company.CompanyId);
    //        parameters.Add(name: "branchId", value: branchId);
    //    }

    //    int rowaffected = 0;
    //    using (var connection = this.OpenConnection())
    //    {
    //        rowaffected = connection.Execute(
    //        sql,
    //        param: parameters,
    //        commandType: CommandType.Text);
    //    }

    //    if (rowaffected > 0 && (companyId == null || companyId < 1))
    //    {
    //        AddOrUpdateLastUserCompanyAndBranch(username);
    //    }

    //    return rowaffected;
    //}

    //public virtual int UpdateUserBranch(string username, string branchCode)
    //{
    //    string aspNetUsers = " [Identity].AspNetUsers ";
    //    if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
    //    {
    //        aspNetUsers = " AspNetUsers ";
    //    }

    //    var parameters = new DynamicParameters();
    //    parameters.Add(name: "branchCode", value: branchCode);


    //    int rowaffected = 0;
    //    using (var connection = this.OpenConnection())
    //    {
    //        var data = connection.Query(() => new
    //        {
    //            CompanyId = default(int),
    //            BranchId = default(int)

    //        }, "SELECT CompanyId, BranchId FROM Branch WHERE BranchCode = @branchCode", parameters).FirstOrDefault();

    //        var updateparameters = new DynamicParameters();
    //        parameters.Add(name: "companyId", value: data.CompanyId);
    //        parameters.Add(name: "branchId", value: data.BranchId);
    //        parameters.Add(name: "userName", value: username);

    //        rowaffected = connection.Execute(
    //        $@"UPDATE {aspNetUsers}
    //       SET 
    //                    CompanyId = {data.CompanyId},
    //        BranchId = {data.BranchId}	            
    //       WHERE UserName = '{username}'",
    //        param: updateparameters,
    //        commandType: CommandType.Text);

    //    }

    //    if (rowaffected > 0)
    //    {
    //        AddOrUpdateLastUserCompanyAndBranch(username);
    //    }

    //    return rowaffected;
    //}

    private string GetUserIdFromUserName(string username)
    {
        string aspNetUsers = " [Identity].AspNetUsers ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUsers = " AspNetUsers ";
        }

        string userId = default!;
        using (var connection = this.OpenConnection())
        {
            string sql = $"SELECT Id FROM {aspNetUsers} where UserName = @username";
            userId = connection.Query<string>(
                sql, new { username }
                ).FirstOrDefault() ?? default!;
        }

        return userId;
    }

    /// <summary>
    /// deletes a user
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public virtual int DeleteUser(string userName)
    {
        string userId = GetUserIdFromUserName(userName);
        if (string.IsNullOrEmpty(userId))
            return 0;

        string previousPassword = " [Identity].PreviousPassword ";


        string aspNetUserRoles = " [Identity].AspNetUserRoles ";


        string aspNetUserClaims = " [Identity].AspNetUserClaims ";


        string aspNetUserLogins = " [Identity].AspNetUserLogins ";


        string aspNetUsers = " [Identity].AspNetUsers ";
        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            aspNetUsers = " AspNetUsers ";
        }

        int rowsAffected = 0;
        using (var connection = this.OpenConnection())
        {
            using (var transaction = BeginSqlTransaction(connection))
            {
                try
                {
                    connection.Execute($"DELETE FROM {previousPassword}  WHERE UserId = @userId", new { userId }, transaction: transaction);
                    connection.Execute($"DELETE FROM {aspNetUserRoles}  WHERE UserId = @userId", new { userId }, transaction: transaction);
                    connection.Execute($"DELETE FROM {aspNetUserClaims}  WHERE UserId = @userId", new { userId }, transaction: transaction);
                    connection.Execute($"DELETE FROM {aspNetUserLogins}  WHERE UserId = @userId", new { userId }, transaction: transaction);
                    rowsAffected = connection.Execute($"DELETE FROM {aspNetUsers}  WHERE Id = @userId", new { userId }, transaction: transaction);


                    CommitSqlTransaction(transaction);

                }
                catch
                {
                    RollbackSqlTransaction(transaction, "DeleteUser", new Exception());
                    rowsAffected = -1;
                }
            }
        }

        return rowsAffected;

    }

    //protected string GetLastUserBranchByCompanyCode(string userName, string companyCode)
    //{
    //    string branchCode = string.Empty;
    //    using (var connection = this.OpenConnection())
    //    {
    //        branchCode = connection.Query<string>(@"SELECT B.BranchCode
    //                        FROM  LastUserCompanyAndBranch  LUCB    
    //                        INNER JOIN  Branch B ON LUCB.BranchId = B.BranchId AND LUCB.CompanyId = B.CompanyId
    //                        INNER JOIN  Company C ON LUCB.CompanyId = C.CompanyId
    //                        WHERE LUCB.UserName = @userName AND C.CompanyCode = @companyCode", new { userName, companyCode }).SingleOrDefault();
    //    }

    //    return branchCode;
    //}

    protected int AddOrUpdateLastUserCompanyAndBranch(string userName)
    {
        (int companyId, int branchId) = GetUsersCompanyAndBranchId(userName);
        if (companyId == 0 || branchId == 0)
            return 0;

        var parameters = new DynamicParameters();
        parameters.Add(name: "userName", value: userName);
        parameters.Add(name: "companyId", value: companyId);
        parameters.Add(name: "branchId", value: branchId);
        parameters.Add(name: "createdBy", value: userName);
        parameters.Add(name: "modifiedby", value: userName);
        parameters.Add(name: "dateCreated", value: DateTime.Now);
        parameters.Add(name: "dateModified", value: DateTime.Now);


        int lastUserCompanyAndBranchId = 0, rowaffected = 0;
        using (var connection = this.OpenConnection())
        {
            lastUserCompanyAndBranchId = connection.Query<int>(@"SELECT LastUserCompanyAndBranchId
                            FROM  LastUserCompanyAndBranch   
                            WHERE UserName = @userName AND CompanyId = @companyId",
                        new { userName, companyId }).SingleOrDefault();

            if (lastUserCompanyAndBranchId < 1)
            {
                const string insertSql = @"INSERT INTO LastUserCompanyAndBranch (UserName,CompanyId,BranchId,DateCreated,CreatedBy,DateModified,ModifiedBy)
                                     VALUES (@userName, @companyId, @branchId, @dateCreated, @createdBy, @dateModified, @modifiedBy)";
                //Insert
                rowaffected = connection.Execute(
                insertSql,
                param: parameters,
                commandType: CommandType.Text);

            }
            else
            {
                parameters.Add(name: "lastUserCompanyAndBranchId", value: lastUserCompanyAndBranchId);
                const string updateSql = @"UPDATE LastUserCompanyAndBranch
                                   SET UserName = @userName
                                      ,CompanyId = @companyId
                                      ,BranchId = @branchId
                                      ,DateCreated = @dateCreated
                                      ,CreatedBy = @createdBy
                                      ,DateModified = @dateModified
                                      ,ModifiedBy = @modifiedBy
                                 WHERE LastUserCompanyAndBranchId = @lastUserCompanyAndBranchId";

                //Update
                rowaffected = connection.Execute(
                updateSql,
                param: parameters,
                commandType: CommandType.Text);
            }
        }

        return rowaffected;
    }

    protected (int? companyId, int? branchId) GetUsersLastCompanyIdAndBranchIdByCompanyCode(string userName, string companyCode)
    {
        using (IDbConnection connection = this.OpenConnection())
        {
            var parameters = new DynamicParameters();
            parameters.Add(name: "userName", value: userName);
            parameters.Add(name: "companyCode", value: companyCode);

            var data = connection.Query(() => new
            {
                CompanyId = default(int),
                BranchId = default(int)

            }, @"SELECT LUCB.CompanyId, LUCB.BranchId FROM LastUserCompanyAndBranch LUCB 
                    INNER JOIN Company C ON C.CompanyId = LUCB.CompanyId
                    WHERE UserName = @userName AND C.CompanyCode = @companyCode", parameters).FirstOrDefault();

            if (data == null)
            {
                return (companyId: null, branchId: null);
            }

            return (companyId: data.CompanyId, branchId: data.BranchId);

        }
    }

    protected virtual (int companyId, int branchId) GetUsersCompanyAndBranchId(string userName)
    {
        using (IDbConnection connection = this.OpenConnection())
        {
            var parameters = new DynamicParameters();
            parameters.Add(name: "userName", value: userName);

            var data = connection.Query(() => new
            {
                CompanyId = default(int),
                BranchId = default(int)

            }, "SELECT CompanyId, BranchId FROM [Identity].[AspNetUsers] WHERE UserName = @userName", parameters).FirstOrDefault();

            if (data == null)
            {
                return (companyId: 0, branchId: 0);
            }

            return (companyId: data.CompanyId, branchId: data.BranchId);

        }

    }

}
