
namespace DataAccess.IdentityModels
{
    using System;

    /// <summary>
    /// Tracks the last Company/Branch a user operated in.
    /// Legacy schema restored so <c>UpdateUser</c> resolves.
    /// </summary>
    public class LastUserCompanyAndBranch
    {
        public int LastUserCompanyAndBranchId { get; set; }

        public string UserName { get; set; }

        public int CompanyId { get; set; }

        public int BranchId { get; set; }

        public DateTime DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateModified { get; set; }

        public string ModifiedBy { get; set; }
    }
}