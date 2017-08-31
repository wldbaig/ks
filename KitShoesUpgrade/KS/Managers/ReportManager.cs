using KS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KS.Managers
{
    public class ReportManager : BaseManager
    {
        

        public List<SaleReport> GetBPSaleReport(Dates model)
        {
            var queryResult = db.Report_BPArticleSaleWithDates(model.StartDate, model.EndDate).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,
                  
                Pairs = c.Pair
               
            }).ToList();

            return queryResult;
        }

        public List<SaleReport> GetCMSaleReport(Dates model)
        {
            var queryResult = db.Report_CMArticleSaleWithDates(model.StartDate, model.EndDate).ToList().Select(c => new SaleReport()
            {
                ArticleID = c.ArticleId,
                ArticleName = c.Article,

                
                Pairs = c.Pair

            }).ToList();

            return queryResult;
        }

    }
}