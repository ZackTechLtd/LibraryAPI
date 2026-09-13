
namespace DataAccess.IdentityModels
{
    /// <summary>
    /// A branch owned by a <see cref="Company"/>.
    /// Legacy schema restored so legacy UserRepository queries resolve.
    /// </summary>
    public class Branch
    {
        public int BranchId { get; set; }

        public int CompanyId { get; set; }

        public string BranchCode { get; set; }

        public string BranchName { get; set; }

        public virtual Company Company { get; set; }
    }
}