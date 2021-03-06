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
    
    public partial class JPArticle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public JPArticle()
        {
            this.JPArticleDetails = new HashSet<JPArticleDetail>();
            this.JPInvoiceDetails = new HashSet<JPInvoiceDetail>();
            this.JPReturnItemDetails = new HashSet<JPReturnItemDetail>();
        }
    
        public int ID { get; set; }
        public string ArticleName { get; set; }
        public string Image { get; set; }
        public int CategoryId { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JPArticleDetail> JPArticleDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JPInvoiceDetail> JPInvoiceDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JPReturnItemDetail> JPReturnItemDetails { get; set; }
    }
}
