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
    
    public partial class CashReciept
    {
        public CashReciept()
        {
            this.CashRecieptDetails = new HashSet<CashRecieptDetail>();
        }
    
        public int CashRecieptID { get; set; }
        public string CashType { get; set; }
        public System.DateTime AddedOn { get; set; }
        public decimal TotalAmount { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual ICollection<CashRecieptDetail> CashRecieptDetails { get; set; }
    }
}
