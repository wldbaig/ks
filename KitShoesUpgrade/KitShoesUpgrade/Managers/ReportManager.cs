using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Managers
{
    public class ReportManager : BaseManager
    {



        public List<OrderReport> GetOrderReport(ReportViewModel model)
        {
            var queryResult = db.Report_ArticleOrderWithDates(model.StartDate, model.EndDate, model.ArticleID, model.CategoryID, model.BuyerID).ToList().Select(c => new OrderReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                Color = c.Color,
                Date = c.AddedDate.Value.ToString("dd-MMM-yyyy"),
                Pairs = c.Pair,
                OrderType = c.OrderType
            }).ToList();

            return queryResult;
        }

        public List<SaleReport> GetSaleReport(ReportViewModel model)
        {
            var queryResult = db.Report_ArticleSaleWithDates(model.StartDate, model.EndDate, model.ArticleID, model.CategoryID, model.CustomerID).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                Color = c.Color,
                Date = c.AddedDate.Value.ToString("dd-MMM-yyyy"),
                Pairs = c.Pair,
                Carton = c.Carton
            }).ToList();

            return queryResult;
        }

        public List<CustomerOutStandingReport> GetCustomerOutStanding(ReportViewModel model)
        {
            var queryResult = db.Report_CustomerOutStanding(model.CategoryID).ToList().Select(c => new CustomerOutStandingReport()
            {
                CustomerName = c.Customer,
                OSAmount = c.OutStandingAmount
            }).ToList();

            return queryResult;
        }

        public List<CustomerAccountDetailRep> GetCustomerAccountDetail(ReportViewModel model)
        {
            var queryResult = db.Report_CustomerAccountDetail(model.StartDate.Date, model.EndDate.Date, model.CustomerID).ToList().Select(c => new CustomerAccountDetailRep()
            {
                InvoiceID = c.InvoiceID,
                InvoicePrice = c.InvoicePrice + c.Freight - c.Claim,
                ReserveID = c.ReserveID,
                ReservePrice = c.ReservePrice,
                ReturnID = c.ReturnID,
                ReturnPrice = c.ReturnPrice,
                TransAmount = c.Amount,
                TransDate = c.CDate.Value.ToString("dd-MMM-yyyy"),
                TransDescription = c.TransactionDetail,
                PreviousOutStanding = c.PreviousOutStanding
            }).OrderBy(c => c.TransDate).ToList();

            return queryResult;
        }

        public List<BuyerAccountDetailRep> GetBuyerAccountDetail(ReportViewModel model)
        {
            var queryResult = db.Report_BuyerAccountDetail(model.StartDate.Date, model.EndDate.Date, model.BuyerID).ToList().Select(c => new BuyerAccountDetailRep()
            {
                OrderID = c.OrderID,
                OrderPrice = c.OrderPrice,
                TransAmount = c.Amount,
                TransDate = c.CDate.Value.ToString("dd-MMM-yyyy")

            }).ToList();

            return queryResult;
        }

        public List<SalesPerDayReport> GetSalePerDay(ReportViewModel model)
        {
            var queryResult = db.Report_SalePerDay(model.StartDate.Date, model.EndDate.Date).ToList().Select(c => new SalesPerDayReport()
            {
                InvoiceID = c.InvoiceID,
                BillDate = c.BillDate.Value.ToString("dd-MMM-yyyy"),
                AmountRecieved = c.AmountRecieved,
                Carton = c.Carton,
                CashCustomer = c.CustomerName,
                CreditCustomer = c.CreditCustomer,
                CustomerType = c.CustomerType,
                Pair = c.Pair,
                TotalBill = c.TotalBill + c.Freight - c.Claim
            }).ToList();

            return queryResult;
        }
    }
}