//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KitShoesUpgrade.Models
{
    using System;
    
    public partial class Report_CustomerAccountDetail_Result
    {
        public Nullable<System.DateTime> CDate { get; set; }
        public string TransactionDetail { get; set; }
        public decimal Amount { get; set; }
        public Nullable<int> InvoiceID { get; set; }
        public Nullable<decimal> InvoicePrice { get; set; }
        public Nullable<decimal> Claim { get; set; }
        public Nullable<decimal> Freight { get; set; }
        public Nullable<decimal> TotalInvoiceBill { get; set; }
        public Nullable<int> ReserveID { get; set; }
        public Nullable<decimal> ReservePrice { get; set; }
        public Nullable<int> ReturnID { get; set; }
        public Nullable<decimal> ReturnPrice { get; set; }
    }
}
