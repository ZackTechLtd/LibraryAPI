
namespace DataAccess.IdentityModels
{
    using System.Collections.Generic;

    /// <summary>
    /// A company that owns one or more branches.
    /// Legacy schema restored so legacy UserRepository queries resolve.
    /// </summary>
    public class Company
    {
        public int CompanyId { get; set; }

        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}