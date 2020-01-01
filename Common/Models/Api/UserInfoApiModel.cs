using System;
namespace Common.Models.Api
{
    using System.Collections.Generic;

    /// <summary>
    /// UserInfoApiModel Class
    /// </summary>
    public class UserInfoApiModel
    {
        /// <summary>
        /// Is the user an administrator
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Is the user an administrator
        /// </summary>
        public bool IsLibrarian { get; set; }

        /// <summary>
        /// List of roles that a user belongs to
        /// </summary>
        public IEnumerable<string> RoleList { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// List of roles the user belongs to
        /// </summary>
        public IEnumerable<ClaimsApiModel> UserRoles { get; set; }

        /// <summary>
        /// List of roles the user belongs to
        /// </summary>
        public IEnumerable<ClaimApiModel> UserClaims { get; set; }

        public Dictionary<string, string> LicensedFeatures { get; set; }
        public Dictionary<string, string> Settings { get; set; }
    }
}
