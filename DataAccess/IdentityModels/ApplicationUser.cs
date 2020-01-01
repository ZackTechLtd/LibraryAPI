

namespace DataAccess.IdentityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// ApplicationUser
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// ApplicationUser Constructor
        /// </summary>
        public ApplicationUser() : base()
        {
            PreviousUserPasswords = new List<PreviousPassword>();
        }

        /// <summary>
        /// LastPasswordChangedDate
        /// </summary>
        public DateTime LastPasswordChangedDate { get; set; }

        /// <summary>
        /// PreviousUserPasswords
        /// </summary>
        public virtual IList<PreviousPassword> PreviousUserPasswords { get; set; }

        /// <summary>
        /// Has the Password Expired
        /// </summary>
        [NotMapped]
        public bool IsPasswordExpired { get; set; }

        /// <summary>
        /// GenerateUserIdentityAsync
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //    // Add custom user claims here
        //    return userIdentity;
        //}

        /// <summary>
        /// GenerateUserIdentityAsync
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
        //    // Add custom user claims here
        //    return userIdentity;
        //}
    }

    /// <summary>
    /// ApplicationUserEx Class - Extends ApplicationUser
    /// </summary>
    public class ApplicationUserEx
    {
        /// <summary>
        /// User
        /// </summary>
        public ApplicationUser User { get; set; }

        
        /// <summary>
        /// Roles
        /// </summary>
        public IList<string> Roles { get; set; }

        /// <summary>
        /// String Roles Delimeted List
        /// </summary>
        public string StrRoles
        {
            get
            {
                string result = string.Empty;

                if (this.Roles != null)
                {
                    foreach (string item in this.Roles)
                    {
                        if (!string.IsNullOrEmpty(result))
                            result += ",";
                        result += item;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// String Roles Lowercase Delimeted List
        /// </summary>
        public string LowerCaseStrRoles
        {
            get
            {
                string result = string.Empty;

                if (this.Roles != null)
                {
                    foreach (string item in this.Roles)
                    {
                        if (!string.IsNullOrEmpty(result))
                            result += ",";
                        result += item.ToLower();
                    }
                }
                return result;
            }
        }

    }
}
