using KS.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace KS.Models
{
    public class ModelMessage
    {
        public bool DisplayMessage { get; set; }
        public string Message { get; set; }
    }

    public class SaleModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public int ShopId { get; set; }
        public int? CategoryId { get; set; }
    }

    public class SaleReport
    {
        public int ArticleID { get; set; }
        public string ArticleName { get; set; }


        public string Shop { get; set; }
        public int? Pairs { get; set; }

    }


    public class LoginViewModel : ModelMessage
    {
        [Display(Name = "Login ID")]//, ResourceType = typeof(Resource))]
        [Required]
        public string LoginID { get; set; }

        [Display(Name = "Password")]//, ResourceType = typeof(Resource))]
        [Required]
        public string Password { get; set; }

        [Display(Name = "RememberMe")]//, ResourceType = typeof(Resource))]
        public bool RememberMe { get; set; }

    }

    public class LoggedInUser
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public string LOGINID { get; set; }
        public DateTime? LASTLOGIN { get; set; }
        public DateTime? PREVIOUSLOGIN { get; set; }
        public int? LOGINCOUNT { get; set; }
        public DateTime? USERSINCE { get; set; }
        public int ROLEID { get; set; }

        // --- Used as role type
        public string ROLE { get; set; }
        public bool ISACTIVE { get; set; }
    }

    public class RoleAndPermissions : ModelMessage
    {
        public Role CRole { get; set; }

        public List<CustomRolePermission> CRolePermission { get; set; }

        //[Display(Name = "ChildRoles", ResourceType = typeof(Resource))]
        //public List<int> RolesList { get; set; }
    }

    public class CustomRolePermission
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public bool canView { get; set; }
        public bool canAdd { get; set; }
        public bool canEdit { get; set; }
        public bool canDelete { get; set; }

        public bool showView { get; set; }
        public bool showAdd { get; set; }
        public bool showEdit { get; set; }
        public bool showDelete { get; set; }
    }

    public class UserViewModel : ModelMessage
    {
        public UserCredential userCredentials { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password Required")]
        [RegularExpression(@"(?=^.{8,}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s)[0-9a-zA-Z!@#$%^&*()]*$", ErrorMessage = "Password must be greater than 7 character and contains at least 1 lowercase letter, 1 uppercase letter and 1 digit")]
        [MinLength(6, ErrorMessage = "Password Length must be greater than 6")]
        public string PASSWORD { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Password Required")]
        [RegularExpression(@"(?=^.{8,}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s)[0-9a-zA-Z!@#$%^&*()]*$", ErrorMessage = "Password must be greater than 7 character and contains at least 1 lowercase letter, 1 uppercase letter and 1 digit")]
        [MinLength(6, ErrorMessage = "Password Length must be greater than 6")]
        [Compare("PASSWORD", ErrorMessage = "Password does not match")]
        public string CONFIRMPASSWORD { get; set; }

    }

    public class KSErrors
    {
        public static Dictionary<eErrorCode, string> Errors = new Dictionary<eErrorCode, string>
        {
            {eErrorCode.DuplicateLoginID, "Login Id already exist"},
            {eErrorCode.IncorrectPassword, "Incorrect Password"},
            {eErrorCode.InvalidCredentials, "Invalid Credentials"},
            {eErrorCode.InvalidLoginID, "Invalid Login ID"},
            {eErrorCode.NoUserFound, "User Not Found"},
        };
    }

    public class KSException : Exception
    {
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public KSException(eErrorCode errorCode)
        {
            if (Enum.IsDefined(typeof(eErrorCode), errorCode))
            {
                ErrorCode = errorCode.ToString();
                ErrorMessage = KSErrors.Errors[errorCode];
            }
        }
    }

    public class ArticleDetailViewModel
    {
        public int ArticleID { get; set; }
        public WHArticleDetail ArticleDetails { get; set; }
        public int Pair { get; set; }

    }

    public class ArticleViewModel
    {
        public WHArticle Article { get; set; }

        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }

    public class ArticleViewM
    {
        public int ID { get; set; }
        public string ArticleName { get; set; }
        public string Category { get; set; }
        public decimal? Price { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
    }

    public class ArtDetV
    {
        public int ArtID { get; set; }
        public int ArtDetID { get; set; }
        public string Color { get; set; }
        public int TotalStock { get; set; }
    }

    public class OrderViewM
    {
        [Display(Name = "Order Number")]
        public int OrderID { get; set; }
        public string OrderStatus { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class InvoiceViewM
    {
        [Display(Name = "Invoice Number")]
        public int InvoiceID { get; set; }
        public string AddedOn { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class InvoiceDetViewM
    {
        [Display(Name = "Invoice Number")]
        public int InvoiceID { get; set; }
        public int InvoiceDetailID { get; set; }
        public string Article { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }

    }

    public class ReturnViewM
    {
        [Display(Name = "Invoice Number")]
        public int ReturnID { get; set; }
        public string AddedOn { get; set; }

    }

    public class ReturnDetViewM
    {
        [Display(Name = "Invoice Number")]
        public int ReturnID { get; set; }
        public int ReturnDetailID { get; set; }
        public string Article { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }

    }

    public class OrderHisViewM
    {
        public int OrderID { get; set; }
        public int OrderHisID { get; set; }
        public string Article { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public string TransferDate { get; set; }
        public string TransferTo { get; set; }
    }

    public class PurchaseRecieptViewModel
    {
        public int InvoiceID { get; set; }
        public int ReturnID { get; set; }
        public int OrderID { get; set; }
        public List<PurchaseReciept> PReciept { get; set; }
    }

    public class PurchaseReciept
    {
        public int Article { get; set; }
        public List<PRDetail> LPRDetail { get; set; }
    }

    public class PRDetail
    {
        public int ArticleDetail { get; set; }
        public int QuantiyAdded { get; set; }

        public int Size6 { get; set; }
        public int Size7 { get; set; }
        public int Size8 { get; set; }
        public int Size9 { get; set; }
        public int Size10 { get; set; }
        public int Size11 { get; set; }
        public int Size12 { get; set; }
        public int Size13 { get; set; }
        public int total { get; set; }
        public decimal Price { get; set; }
    }

    public class RecieveViewModel
    {
        public int OrderID { get; set; }
        public int TransferTo { get; set; }
        public List<OrderArtInfo> OrderInfo { get; set; }
    }

    public class OrderArtInfo
    {
        public int ArticleID { get; set; }
        public List<OrderHis> OrderHis { get; set; }
    }

    public class OrderHis
    {
        public int ArticleDetID { get; set; }
        public string ColorName { get; set; }
        public int TotalOrder { get; set; }
        public int TotalRecieved { get; set; }
        public int OrderDetail { get; set; }
    }

    public class ArtilceList
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class EditOrderViewModel
    {
        public int OrderID { get; set; }
        public Order Order { get; set; }
        public List<ArtilceList> ArtilceList { get; set; }
        public List<WHArticle> Article { get; set; }
    }

    public class EditInvoiceViewModel
    {
        public int InvoiceID { get; set; }
        public BPInvoice BPInvoice { get; set; }
        public CMInvoice CMInvoice { get; set; }
        public TSInvoice TSInvoice { get; set; }
        public List<ArtilceList> ArtilceList { get; set; }
        public List<BPArticle> BPArticle { get; set; }
        public List<CMArticle> CMArticle { get; set; }
        public List<TSArticle> TSArticle { get; set; }
    }
}