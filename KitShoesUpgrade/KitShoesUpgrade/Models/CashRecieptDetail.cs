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
    
    public partial class CashRecieptDetail
    {
        public int CashRecieptDetailID { get; set; }
        public int CashRecieptID { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public Nullable<int> BuyerID { get; set; }
        public decimal Amount { get; set; }
    
        public virtual Buyer Buyer { get; set; }
        public virtual CashReciept CashReciept { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
