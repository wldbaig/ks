﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class KSEntities : DbContext
    {
        public KSEntities()
            : base("name=KSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<ArticleDetail> ArticleDetails { get; set; }
        public virtual DbSet<Buyer> Buyers { get; set; }
        public virtual DbSet<BuyerAccount> BuyerAccounts { get; set; }
        public virtual DbSet<BuyerAccountDetail> BuyerAccountDetails { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public virtual DbSet<CustomerAccountDetail> CustomerAccountDetails { get; set; }
        public virtual DbSet<CustomerCategory> CustomerCategories { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderHistory> OrderHistories { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<ReserveSaleDetail> ReserveSaleDetails { get; set; }
        public virtual DbSet<ReturnItem> ReturnItems { get; set; }
        public virtual DbSet<ReturnItemDetail> ReturnItemDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<UserCredential> UserCredentials { get; set; }
        public virtual DbSet<ReserveSale> ReserveSales { get; set; }
        public virtual DbSet<CashReciept> CashReciepts { get; set; }
        public virtual DbSet<CashRecieptDetail> CashRecieptDetails { get; set; }
    
        public virtual ObjectResult<Report_ArticleOrderWithDates_Result> Report_ArticleOrderWithDates(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate, Nullable<int> articleID, Nullable<int> categoryID, Nullable<int> buyerID)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            var articleIDParameter = articleID.HasValue ?
                new ObjectParameter("ArticleID", articleID) :
                new ObjectParameter("ArticleID", typeof(int));
    
            var categoryIDParameter = categoryID.HasValue ?
                new ObjectParameter("CategoryID", categoryID) :
                new ObjectParameter("CategoryID", typeof(int));
    
            var buyerIDParameter = buyerID.HasValue ?
                new ObjectParameter("BuyerID", buyerID) :
                new ObjectParameter("BuyerID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_ArticleOrderWithDates_Result>("Report_ArticleOrderWithDates", startDateParameter, endDateParameter, articleIDParameter, categoryIDParameter, buyerIDParameter);
        }
    
        public virtual ObjectResult<Report_ArticleSaleWithDates_Result> Report_ArticleSaleWithDates(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate, Nullable<int> articleID, Nullable<int> categoryID, Nullable<int> customerID)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            var articleIDParameter = articleID.HasValue ?
                new ObjectParameter("ArticleID", articleID) :
                new ObjectParameter("ArticleID", typeof(int));
    
            var categoryIDParameter = categoryID.HasValue ?
                new ObjectParameter("CategoryID", categoryID) :
                new ObjectParameter("CategoryID", typeof(int));
    
            var customerIDParameter = customerID.HasValue ?
                new ObjectParameter("CustomerID", customerID) :
                new ObjectParameter("CustomerID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_ArticleSaleWithDates_Result>("Report_ArticleSaleWithDates", startDateParameter, endDateParameter, articleIDParameter, categoryIDParameter, customerIDParameter);
        }
    
        public virtual ObjectResult<Report_CustomerOutStanding_Result> Report_CustomerOutStanding(Nullable<int> categoryCustomer)
        {
            var categoryCustomerParameter = categoryCustomer.HasValue ?
                new ObjectParameter("CategoryCustomer", categoryCustomer) :
                new ObjectParameter("CategoryCustomer", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_CustomerOutStanding_Result>("Report_CustomerOutStanding", categoryCustomerParameter);
        }
    
        public virtual ObjectResult<Report_BuyerAccountDetail_Result> Report_BuyerAccountDetail(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate, Nullable<int> buyerID)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            var buyerIDParameter = buyerID.HasValue ?
                new ObjectParameter("BuyerID", buyerID) :
                new ObjectParameter("BuyerID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_BuyerAccountDetail_Result>("Report_BuyerAccountDetail", startDateParameter, endDateParameter, buyerIDParameter);
        }
    
        public virtual ObjectResult<Report_SalePerDay_Result> Report_SalePerDay(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_SalePerDay_Result>("Report_SalePerDay", startDateParameter, endDateParameter);
        }
    
        public virtual ObjectResult<Report_CustomerAccountDetail_Result> Report_CustomerAccountDetail(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate, Nullable<int> customerID)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            var customerIDParameter = customerID.HasValue ?
                new ObjectParameter("CustomerID", customerID) :
                new ObjectParameter("CustomerID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Report_CustomerAccountDetail_Result>("Report_CustomerAccountDetail", startDateParameter, endDateParameter, customerIDParameter);
        }
    }
}