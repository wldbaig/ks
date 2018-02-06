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
    
    public partial class ReserveSaleDetail
    {
        public int ID { get; set; }
        public int ReserveSaleID { get; set; }
        public int ArticleID { get; set; }
        public int ArticleDetailID { get; set; }
        public Nullable<int> ArticlePairs { get; set; }
        public Nullable<int> ArticleCartons { get; set; }
        public decimal Price { get; set; }
    
        public virtual Article Article { get; set; }
        public virtual ArticleDetail ArticleDetail { get; set; }
        public virtual ReserveSale ReserveSale { get; set; }
    }
}