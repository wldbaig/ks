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
    using System.Collections.Generic;
    
    public partial class CustomerAccountDetail
    {
        public int ID { get; set; }
        public int CAccountID { get; set; }
        public decimal TotalAmount { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> InvoiceID { get; set; }
        public string Description { get; set; }
        public Nullable<int> ReturnID { get; set; }
        public Nullable<int> ReserveID { get; set; }
        public Nullable<int> CashRecieptID { get; set; }
        public Nullable<decimal> PreviousOutStanding { get; set; }
    
        public virtual CustomerAccount CustomerAccount { get; set; }
    }
}