//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CMReturnItemDetail
    {
        public int ID { get; set; }
        public int CMReturnItemID { get; set; }
        public int ArticleID { get; set; }
        public int ArticleDetailID { get; set; }
        public int ArticlePairs { get; set; }
        public decimal Price { get; set; }
    
        public virtual CMArticle CMArticle { get; set; }
        public virtual CMArticleDetail CMArticleDetail { get; set; }
        public virtual CMReturnItem CMReturnItem { get; set; }
    }
}
