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
    
    public partial class OrderHistory
    {
        public int OrderHistoryID { get; set; }
        public int OrderID { get; set; }
        public int OrderDetailID { get; set; }
        public int ArticleID { get; set; }
        public int ArticleDetailID { get; set; }
        public int ArticlePairs { get; set; }
        public System.DateTime RecievedOn { get; set; }
    
        public virtual Order Order { get; set; }
        public virtual OrderDetail OrderDetail { get; set; }
    }
}
