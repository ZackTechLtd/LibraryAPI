using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Api
{
    public class Role
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public User User { get; set; }
    }

    /// <summary>
    /// RolesApiModel Class
    /// </summary>
    public class RolesApiModel : ApiModel
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string RoleId { get; set; }
        /// <summary>
        /// Name of the role
        /// </summary>
        public string RoleName { get; set; }
    }

    public class ApplicationRolesApiModel : ApiModel
    {
        /// <summary>
        /// Roles the application user is a member of 
        /// </summary>
        public IEnumerable<RolesApiModel> UserRoles { get; set; }
        /// <summary>
        /// List of all application roles
        /// </summary>
        public IEnumerable<RolesApiModel> AllRoles { get; set; }
    }

    public class ListRoleApiModel : ApiModel
    {
        public IEnumerable<RolesApiModel> Roles { get; set; }
    }
}
