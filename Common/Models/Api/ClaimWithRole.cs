

namespace Common.Models.Api
{
    using System.Collections.Generic;
    public class ClaimWithRole
    {
        public string ClaimId { get; set; }

        public string ClaimValue { get; set; }

        public string Name { get; set; }
    }

    public class AllClaimsWithRolesApiModel : ApiModel
    {
        public IEnumerable<ClaimWithRole> AllClaimsWithRoles { get; set; }
    }
}
