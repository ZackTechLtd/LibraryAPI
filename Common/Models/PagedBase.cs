using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// PagedBase Class
    /// </summary>
    public class PagedBase
    {
        public int? PageNum { get; set; }
        /// <summary>
        /// Number of records to display on a page
        /// </summary>
        public int? PageSize { get; set; }
        /// <summary>
        /// Column to order by
        /// </summary>
        public int? OrderBy { get; set; }
        /// <summary>
        /// Sort assending or decending
        /// </summary>
        public int SortOrder { get; set; }
        /// <summary>
        /// Text used to filter results
        /// </summary>
        public string SearchText { get; set; }

        public IEnumerable<string> FooterFilters { get; set; }

        public PagedBase()
        {
            PageNum = 1;
            PageSize = 10;
            OrderBy = 1;
            SortOrder = 1;
            SearchText = "";
        }
    }

    
}
