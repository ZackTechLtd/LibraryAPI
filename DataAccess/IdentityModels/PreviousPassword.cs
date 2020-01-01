

namespace DataAccess.IdentityModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// PreviousPassword Class
    /// </summary>
    public class PreviousPassword
    {
        /// <summary>
        /// PreviousPassword Constructor
        /// </summary>
        public PreviousPassword()
        {
            CreateDate = DateTimeOffset.Now;
        }

        /// <summary>
        /// PaswordHash
        /// </summary>
        //[Key, Column(Order = 0)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// CreateDate
        /// </summary>
        public DateTimeOffset CreateDate { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        //[Key, Column(Order = 1)]
        public string UserId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        public virtual ApplicationUser User { get; set; }

    }
}
