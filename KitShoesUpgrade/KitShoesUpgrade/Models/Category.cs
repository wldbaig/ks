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
    
    public partial class Category
    {
        public Category()
        {
            this.Articles = new HashSet<Article>();
        }
    
        public int ID { get; set; }
        public string CategoryName { get; set; }
    
        public virtual ICollection<Article> Articles { get; set; }
    }
}
