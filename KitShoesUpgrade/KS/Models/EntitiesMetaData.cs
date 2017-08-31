using System.ComponentModel.DataAnnotations;

namespace KS.Models
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

    [MetadataType(typeof(ArticleDetailMetaData))]
    public partial class WHArticleDetail : ModelMessage { }

    public class ArticleDetailMetaData
    { 
        [Display(Name = "Color")]
        public int ColorId { get; set; }
    }
     
}