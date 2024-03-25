using System;
using System.Collections.Generic;

namespace Common.Models.Api
{
    public class ApiItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ApiItemCollectionApiModel : ApiModel
    {
        /// <summary>
        /// List of Results
        /// </summary>
        public IEnumerable<ApiItem> Results { get; set; }

    }
}
