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
    
    public partial class Order
    {
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderHistories = new HashSet<OrderHistory>();
        }
    
        public int OrderID { get; set; }
        public Nullable<int> BuyerID { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public decimal AmountRecieved { get; set; }
    
        public virtual Buyer Buyer { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderHistory> OrderHistories { get; set; }
    }
}
