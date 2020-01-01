using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Models.Api
{
    public class LibraryBookApiModel : ApiModel
    {
        public string LibraryBookCode { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        public bool IsStolen { get; set; }
        public bool IsLost { get; set; }
        [Required]
        public int CopyNumber { get; set; }
        [Display(Name = "DateCreated")]
        public DateTime DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateModified { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class LibraryBookListItem : LibraryBookApiModel
    {
        public int RowNumber { get; set; }

        public int TotalRows { get; set; }

    }

    /// <summary>
    /// LibraryBookStatusPageApiModel Class
    /// </summary>
    public class LibraryBookPageApiModel : ApiModel
    {
        /// <summary>
        /// Count of search results
        /// </summary>
        public int SearchResultCount { get; set; }
        /// <summary>
        /// List of Results
        /// </summary>
        public IEnumerable<LibraryBookApiModel> Results { get; set; }

    }
}

