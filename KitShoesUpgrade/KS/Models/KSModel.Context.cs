﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class KitSEntities : DbContext
    {
        public KitSEntities()
            : base("name=KitSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BPArticle> BPArticles { get; set; }
        public virtual DbSet<BPArticleDetail> BPArticleDetails { get; set; }
        public virtual DbSet<BPInvoice> BPInvoices { get; set; }
        public virtual DbSet<BPInvoiceDetail> BPInvoiceDetails { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CMArticle> CMArticles { get; set; }
        public virtual DbSet<CMArticleDetail> CMArticleDetails { get; set; }
        public virtual DbSet<CMInvoice> CMInvoices { get; set; }
        public virtual DbSet<CMInvoiceDetail> CMInvoiceDetails { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderHistory> OrderHistories { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<TSArticle> TSArticles { get; set; }
        public virtual DbSet<TSArticleDetail> TSArticleDetails { get; set; }
        public virtual DbSet<TSInvoice> TSInvoices { get; set; }
        public virtual DbSet<TSInvoiceDetail> TSInvoiceDetails { get; set; }
        public virtual DbSet<UserCredential> UserCredentials { get; set; }
        public virtual DbSet<WHArticle> WHArticles { get; set; }
        public virtual DbSet<WHArticleDetail> WHArticleDetails { get; set; }
        public virtual DbSet<BPReturnItem> BPReturnItems { get; set; }
        public virtual DbSet<BPReturnItemDetail> BPReturnItemDetails { get; set; }
        public virtual DbSet<CMReturnItem> CMReturnItems { get; set; }
        public virtual DbSet<CMReturnItemDetail> CMReturnItemDetails { get; set; }
        public virtual DbSet<TSReturnItem> TSReturnItems { get; set; }
        public virtual DbSet<TSReturnItemDetail> TSReturnItemDetails { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
    
        public virtual ObjectResult<Report_BPArticleSaleWithDates_Result> Report_BPArticleSaleWithDates(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_BPArticleSaleWithDates_Result>("Report_BPArticleSaleWithDates", startDateParameter, endDateParameter);
        }
    
        public virtual ObjectResult<Report_CMArticleSaleWithDates_Result> Report_CMArticleSaleWithDates(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_CMArticleSaleWithDates_Result>("Report_CMArticleSaleWithDates", startDateParameter, endDateParameter);
        }
    }
}
