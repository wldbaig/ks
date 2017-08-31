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
    
    public partial class BPArticleDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BPArticleDetail()
        {
            this.BPInvoiceDetails = new HashSet<BPInvoiceDetail>();
            this.BPReturnItemDetails = new HashSet<BPReturnItemDetail>();
        }
    
        public int ID { get; set; }
        public int BPArticleId { get; set; }
        public int ColorId { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }
        public int TotalStock { get; set; }
    
        public virtual BPArticle BPArticle { get; set; }
        public virtual Color Color { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BPInvoiceDetail> BPInvoiceDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BPReturnItemDetail> BPReturnItemDetails { get; set; }
    }
}
