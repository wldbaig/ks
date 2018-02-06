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
    
    public partial class Article
    {
        public Article()
        {
            this.ArticleDetails = new HashSet<ArticleDetail>();
            this.InvoiceDetails = new HashSet<InvoiceDetail>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.ReserveSaleDetails = new HashSet<ReserveSaleDetail>();
            this.ReturnItemDetails = new HashSet<ReturnItemDetail>();
        }
    
        public int ID { get; set; }
        public string ArticleName { get; set; }
        public string Image { get; set; }
        public int CategoryId { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public int PairInCarton { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedOn { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual ICollection<ArticleDetail> ArticleDetails { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ReserveSaleDetail> ReserveSaleDetails { get; set; }
        public virtual ICollection<ReturnItemDetail> ReturnItemDetails { get; set; }
    }
}