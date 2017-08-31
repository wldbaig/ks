using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Models
{
    [MetadataType(typeof(UserCredentailsMetaData))]
    public partial class UserCredential : ModelMessage { }

    public class UserCredentailsMetaData
    {
        [Display(Name = "Role")]
        [Required(ErrorMessage = "Role Required")]
        public int RoleID { get; set; }

        [Display(Name = "Login ID")]
        [Required(ErrorMessage = "LoginID Required")]
        public string LoginID { get; set; }

    }

    [MetadataType(typeof(ArticleMetaData))]
    public partial class Article : ModelMessage { }

    public class ArticleMetaData
    {
        [Display(Name = "Pairs Per Carton")]
        [Required]
        public int PairInCarton { get; set; }

    }

    [MetadataType(typeof(ArticleDetailMetaData))]
    public partial class ArticleDetail : ModelMessage { }

    public class ArticleDetailMetaData
    {
        [Display(Name = "Pairs")]
        [Required]
        public int Pairs { get; set; }

        [Display(Name = "Cartons")]
        [Required]
        public int Carton { get; set; }

        [Display(Name = "Color")]
        public int ColorId { get; set; }
    }

    [MetadataType(typeof(CustomerMetaData))]
    public partial class Customer : ModelMessage { }

    public class CustomerMetaData {

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Address")]
        [Required]
        public string Address { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Display(Name = "Customer Category")]
        public int CustomerCategoryID { get; set; }

    }

}