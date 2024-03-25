

namespace Common.Models.Api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class LibraryBookStatusApiModel : ApiModel
    {
        public string LibraryBookStatusCode { get; set; }

        [Required]
        public virtual LibraryBookApiModel LibraryBook { get; set; }
        [Required]
        public virtual LibraryUserApiModel LibraryUser { get; set; }
        [Required]
        public DateTime? DateCheckedOut { get; set; }
        public DateTime? DateReturned { get; set; }
        [Display(Name = "DateCreated")]
        public DateTime DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateModified { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class LibraryBookStatusListItem : LibraryBookStatusApiModel
    {
        public int RowNumber { get; set; }

        public int TotalRows { get; set; }

    }

    /// <summary>
    /// LibraryBookStatusPageApiModel Class
    /// </summary>
    public class LibraryBookStatusPageApiModel : ApiModel
    {
        /// <summary>
        /// Count of search results
        /// </summary>
        public int SearchResultCount { get; set; }
        /// <summary>
        /// List of Results
        /// </summary>
        public IEnumerable<LibraryBookStatusApiModel> Results { get; set; }

    }
}
