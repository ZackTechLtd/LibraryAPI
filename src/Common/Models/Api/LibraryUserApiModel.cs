

namespace Common.Models.Api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public class LibraryUserApiModel : ApiModel
    {
        public string LibraryUserCode { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Telephone")]
        public string PhoneNumber { get; set; }
        [Display(Name = "MobileNumber")]
        public string MobilePhoneNumber { get; set; }

        [Display(Name = "EmailAddress")]
        public string Email { get; set; }
        [Display(Name = "AlternativePhoneNumber")]
        public string AlternativePhoneNumber { get; set; }
        [Display(Name = "AlternativeEmail")]
        public string AlternativeEmail { get; set; }

        [Required]
        [Display(Name = "AddressLine1")]
        public string AddressLine1 { get; set; }
        [Display(Name = "AddressLine2")]
        public string AddressLine2 { get; set; }
        [Display(Name = "AddressLine3")]
        public string AddressLine3 { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "County")]
        public string County { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; }

        [NotMapped]
        [Display(Name = "Address")]
        public string Address
        {
            get
            {
                string address = string.Empty;
                string[] addressParts = { AddressLine1, AddressLine2, AddressLine3, City, County, Postcode };
                foreach (string item in addressParts)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    if (!string.IsNullOrEmpty(address))
                        address += ",";

                    address += item;
                }
                return address;
            }
        }

        [Display(Name = "Name")]
        public string LibraryUserName
        {
            get
            {
                if (!string.IsNullOrEmpty(Title))
                    return $"{Title} {Name}";
                else
                    return Name;
            }
        }

        #region GDPR
        [Display(Name = "InformedDate")]
        public DateTime? GDPRInformedDate { get; set; }
        [Display(Name = "InformedBy")]
        public string GDPRInformedBy { get; set; }
        [Display(Name = "HowInformed")]
        public string GDPRHowInformed { get; set; }
        [Display(Name = "Notes")]
        public string GDPRNotes { get; set; }
        [Display(Name = "LibraryUserByPost")]
        public bool LibraryUserByPost { get; set; }
        public DateTime? LibraryUserByPostConsentDate { get; set; }
        [Display(Name = "LibraryUserByEmail")]
        public bool LibraryUserByEmail { get; set; }
        public DateTime? LibraryUserByEmailConsentDate { get; set; }
        [Display(Name = "LibraryUserByPhone")]
        public bool LibraryUserByPhone { get; set; }
        public DateTime? LibraryUserByPhoneConsentDate { get; set; }
        [Display(Name = "SMS")]
        public bool LibraryUserBySMS { get; set; }
        public DateTime? LibraryUserBySMSConsentDate { get; set; }
        #endregion

        [Display(Name = "DateCreated")]
        public DateTime DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateModified { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class LibraryUserListItem : LibraryUserApiModel
    {
        public int RowNumber { get; set; }

        public int TotalRows { get; set; }

    }

    /// <summary>
    /// LibraryUserPageApiModel Class
    /// </summary>
    public class LibraryUserPageApiModel : ApiModel
    {
        /// <summary>
        /// Count of search results
        /// </summary>
        public int SearchResultCount { get; set; }
        /// <summary>
        /// List of Results
        /// </summary>
        public IEnumerable<LibraryUserApiModel> Results { get; set; }

    }
}
