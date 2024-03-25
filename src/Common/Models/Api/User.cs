using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Api
{
    public class User
    {
        public string UserName { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public int CompanyId { get; set; }

        public string BranchCode { get; set; }

        public string XmlUserPermissionIds { get; set; }

        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public DateTime DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateModified { get; set; }

        public string ModifiedBy { get; set; }

        



    }

    public class UserListItem : User
    {
        public int RowNumber { get; set; }

        public int TotalRows { get; set; }

    }
}
