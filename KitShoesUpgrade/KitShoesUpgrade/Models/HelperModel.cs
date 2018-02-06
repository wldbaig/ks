using KitShoesUpgrade.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Models
{
    public class ModelMessage
    {
        public bool DisplayMessage { get; set; }
        public string Message { get; set; }
    }

    public class ReportViewModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        public int? CategoryID { get; set; }

        public int? ArticleID { get; set; }

        [Required]
        public int? CustomerID { get; set; }
        public int? BuyerID { get; set; }

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
        public Nullable<System.DateTime> LASTLOGIN { get; set; }
        public Nullable<System.DateTime> PREVIOUSLOGIN { get; set; }
        public Nullable<int> LOGINCOUNT { get; set; }
        public Nullable<System.DateTime> USERSINCE { get; set; }
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
        public ArticleDetail ArticleDetails { get; set; }

        public int Pair { get; set; }
        public int Carton { get; set; }
        public int previousColor { get; set; }
    }

    public class ArticleViewModel
    {
        public Article Article { get; set; }

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
        public int Carton { get; set; }
        public int Pairs { get; set; }
        public int TotalStock { get; set; }
    }

    public class OrderViewM
    {
        public int OrderID { get; set; }
        public string BuyerName { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderHisViewM
    {
        public int OrderID { get; set; }
        public int OrderHisID { get; set; }
        public string Article { get; set; }

        public string Color { get; set; }
        public int Quantity { get; set; }
        public DateTime RecievedOn { get; set; }
    }


    public class InvoiceViewM
    {
        public int InvoiceID { get; set; }
        public string AddedOn { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerType { get; set; }
        public string CashCustomer { get; set; }
        public string CreditCustomer { get; set; }
    }


    public class CustomerAccountViewModel
    {
        public int CustomerID { get; set; }
        public decimal OutStanding { get; set; }
        public int AccountID { get; set; }
        public decimal Amount { get; set; }
    }


    public class PurchaseViewModel
    {
        [Required]
        [Display(Name = "Buyer")]
        public int BuyerID { get; set; }
        [Required]
        [Display(Name = "Order Type")]
        public int OrderType { get; set; }
        [Required]
        public List<int> Article { get; set; }
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }
    }

    public class PurchaseRecieptViewModel
    {
        public int InvoiceID { get; set; }
        public string Date { get; set; }
        public int ReturnID { get; set; }

        public int ReserveID { get; set; }
        public int OrderID { get; set; }
        public Customer CustomerInfo { get; set; }
        public Buyer BuyerInfo { get; set; }
        public List<PurchaseReciept> PReciept { get; set; }
        public decimal AmountRecieved { get; set; }

        [Display(Name = "Claim")]
        public decimal Discount { get; set; }
        public decimal Freight { get; set; }

        public decimal PreviousOutStanding { get; set; }

    }

    public class PurchaseReciept
    {
        public int Article { get; set; }
        public decimal? UnitPrice { get; set; }
        public List<PRDetail> LPRDetail { get; set; }
    }

    public class PRDetail
    {
        public int ArticleDetail { get; set; }
        public int QuantiyAdded { get; set; }
        public int CartonAdded { get; set; }
        public decimal Price { get; set; }
    }

    public class RecieveViewModel
    {
        public int OrderID { get; set; }
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

    public class InvoiceViewModel
    {
        [Required]
        public string CustomerType { get; set; }


        [Display(Name = "Cash Customer")]
        public string CashCustomerName { get; set; }

        [Required]
        public List<int> Article { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Claim")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Freight Amount")]
        public decimal Freight { get; set; }

        [Display(Name = "Credit Customer")]
        public int CreditCustomerID { get; set; }

    }

    public class CashCustViewModel
    {
        [Required]
        public List<int> Custs { get; set; }
    }

    public class AddArticleInInvoice
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public List<int> Articles { get; set; }
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }
    }

    public class EditInvoiceViewModel
    {
        public int InvoiceID { get; set; }

        public Invoice Invoice { get; set; }

        public List<ArtilceList> ArtilceList { get; set; }
        public List<Article> Article { get; set; }
    }

    public class ArtilceList
    {
        public int ID { get; set; }
        public string Name { get; set; }

    }

    public class CustomerList
    {
        public int ID { get; set; }
        public string Name { get; set; }

    }

    public class OrderReport
    {
        public int ArticleID { get; set; }
        public string ArticleName { get; set; }
        public string Color { get; set; }
        public string Date { get; set; }
        public int? Pairs { get; set; }
        public string OrderType { get; set; }
    }

    public class SaleReport
    {
        public int ArticleID { get; set; }
        public string ArticleName { get; set; }
        public string Color { get; set; }
        public string Date { get; set; }
        public int? Pairs { get; set; }
        public int? Carton { get; set; }
    }

    public class ReturnItemM
    {
        public int ReturnItemID { get; set; }
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public decimal Price { get; set; }
        public string AddedOn { get; set; }
    }

    public class ReturnItemDetM
    {
        public int ID { get; set; }
        public int ReturnItemID { get; set; }
        public string ArticleName { get; set; }
        public int ArticleID { get; set; }
        public int ArticleDetailID { get; set; }
        public string ColorName { get; set; }
        public int? Pairs { get; set; }
        public int? Cartons { get; set; }
        public decimal Price { get; set; }
    }

    public class ReturnViewModel
    {
        [Required]
        public string CustomerType { get; set; }

        [Display(Name = "Cash Customer")]
        public string CashCustomerName { get; set; }

        [Required]
        public List<int> Article { get; set; }

        [Display(Name = "Credit Customer")]
        public int CreditCustomerID { get; set; }

        public int Claim { get; set; }

    }


    public class ReserveViewM
    {
        public int ReserveSaleID { get; set; }
        public string AddedOn { get; set; }
        public decimal TotalPrice { get; set; }
        public string CreditCustomer { get; set; }
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

    public class ReserveDetViewM
    {
        [Display(Name = "Invoice Number")]
        public int ReserveID { get; set; }
        public int ReserveDetailID { get; set; }
        public string Article { get; set; }
        public string Color { get; set; }
        public int? Pair { get; set; }
        public int? Carton { get; set; }
    }

    public class ReserveViewModel
    {

        [Required]
        public List<int> Article { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }
        [Display(Name = "Credit Customer")]
        public int CreditCustomerID { get; set; }

    }

    public class EditReserveSaleViewModel
    {
        public int ReserveSaleID { get; set; }
        public ReserveSale ResSale { get; set; }
        public List<ArtilceList> ArtilceList { get; set; }
        public List<Article> Article { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public class EditOrderViewModel
    {
        public int OrderID { get; set; }
        public Order order { get; set; }
        public List<ArtilceList> ArtilceList { get; set; }
        public List<Article> Article { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public class CashRecViewM
    {
        public int CashRecID { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CashRecDetViewM
    {
        public int CashRecID { get; set; }
        public int CashRecDetID { get; set; }
        public string Name { get; set; }
        public int? CutomerId { get; set; }
        public decimal Amount { get; set; }

    }

    public class CustomerOutStandingReport
    {
        public string CustomerName { get; set; }
        public decimal OSAmount { get; set; }
    }

    public class CustomerAccountDetailRep
    {
        public string TransDate { get; set; }
        public string TransDescription { get; set; }
        public decimal TransAmount { get; set; }
        public int? InvoiceID { get; set; }
        public decimal? InvoicePrice { get; set; }
        public int? ReserveID { get; set; }
        public decimal? ReservePrice { get; set; }
        public int? ReturnID { get; set; }
        public decimal? ReturnPrice { get; set; }
        public decimal? PreviousOutStanding { get; set; }
    }

    public class CustAccDetailRep
    {
        public List<CustomerAccountDetailRep> accList { get; set; }

        public decimal totalOutstanding { get; set; }
    }


    public class BuyerAccDetailRep
    {
        public List<BuyerAccountDetailRep> accList { get; set; }

        public decimal totalOutstanding { get; set; }
    }

    public class BuyerAccountDetailRep
    {
        public string TransDate { get; set; }
        public string TransDescription { get; set; }
        public decimal TransAmount { get; set; }
        public int? OrderID { get; set; }
        public decimal? OrderPrice { get; set; }
      
    }

    public class SalesPerDayReport
    {
        public int InvoiceID { get; set; }
        public string BillDate { get; set; }
        public string CustomerType { get; set; }
        public string CashCustomer { get; set; }
        public string CreditCustomer { get; set; }

        public int? Pair { get; set; }
        public int? Carton { get; set; }
        public decimal TotalBill { get; set; }
        public decimal AmountRecieved { get; set; }
    }

}