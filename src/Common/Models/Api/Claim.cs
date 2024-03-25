

namespace Common.Models.Api
{
    using System.Collections.Generic;
    public class Claim
    {
        public string ClaimId { get; set; }
        public string ClaimValue { get; set; }

        public User User { get; set; }
    }

    /// <summary>
    /// ClaimsApiModel Class
    /// </summary>
    public class ClaimsApiModel
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string ClaimId { get; set; }
        /// <summary>
        /// Name of the role
        /// </summary>
        public string ClaimValue { get; set; }
    }

    public class ClaimApiModel
    {

        /// <summary>
        /// Name of the role
        /// </summary>
        public string ClaimName { get; set; }
    }

    public class ListClaimsApiModel : ApiModel
    {
        public IEnumerable<ClaimsApiModel> Claims { get; set; }
    }

    public class ListClaimsForRoleApiModel : ApiModel
    {
        public string Rolename { get; set; }
        public IEnumerable<ClaimsApiModel> Claims { get; set; }
    }
}
