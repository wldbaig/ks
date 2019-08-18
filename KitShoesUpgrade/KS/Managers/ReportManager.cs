using KS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KS.Managers
{
    public class ReportManager : BaseManager
    {

        public List<SaleReport> GetBPSaleReport(SaleModel model)
        {
            var queryResult = db.Report_BPArticleSaleWithDates(model.StartDate, model.EndDate, model.CategoryId).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                Pairs = c.Pair
            }).ToList();

            return queryResult;
        }

        public List<SaleReport> GetCMSaleReport(SaleModel model)
        {
            var queryResult = db.Report_CMArticleSaleWithDates(model.StartDate, model.EndDate, model.CategoryId).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                Pairs = c.Pair
            }).ToList();

            return queryResult;
        }

        public List<SaleReport> GetTSSaleReport(SaleModel model)
        {
            var queryResult = db.Report_TSArticleSaleWithDates(model.StartDate, model.EndDate, model.CategoryId).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                Pairs = c.Pair
            }).ToList();

            return queryResult;
        }

        public List<SaleReport> GetJPSaleReport(SaleModel model)
        {
            var queryResult = db.Report_JPArticleSaleWithDates(model.StartDate, model.EndDate, model.CategoryId).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                Pairs = c.Pair
            }).ToList();

            return queryResult;
        }

        public List<SaleReport> GetArticleSaleReport(SaleModel model)
        {
            var queryResult = db.Report_ArticleSaleWithCreationDates(model.StartDate.Date, model.EndDate.Date, model.ShopId).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ID,
                ArticleName = c.ArticleName,
                Pairs = c.PairSale,
                Shop = c.Shop

            }).ToList();

            return queryResult;
        }

    }
}