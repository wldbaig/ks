using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Managers;
using KitShoesUpgrade.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.ReportManagement
{
    public class ReportController : BaseController
    {
        ReportManager _manager;
 
        public ReportController()
        {
            _manager = new ReportManager();
        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult Order()
        {
            ViewBag.Article = new SelectList(db.Articles.Where(c => c.IsActive == true).ToList(), "ID", "ArticleName");
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            ViewBag.Buyer = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(new ReportViewModel());
        }

        [HttpPost]
        public ActionResult Order(ReportViewModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;
            try
            {

                var model1 = _manager.GetOrderReport(model);
                return View("OrderDetailReport", model1);

            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }

            ViewBag.Article = new SelectList(db.Articles.Where(c => c.IsActive == true).ToList(), "ID", "ArticleName");
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            ViewBag.Buyer = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");

            return View(model);
        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult Sale()
        {
            ViewBag.Article = new SelectList(db.Articles.Where(c => c.IsActive == true).ToList(), "ID", "ArticleName");
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            ViewBag.Customers = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(new ReportViewModel());
        }

        [HttpPost]
        public ActionResult Sale(ReportViewModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {

                var model1 = _manager.GetSaleReport(model);
                return View("SaleDetailReport", model1);

            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Article = new SelectList(db.Articles.Where(c => c.IsActive == true).ToList(), "ID", "ArticleName");
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            ViewBag.Customers = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(model);
        }

        public ActionResult CustomerOutStanding()
        {
            ViewBag.CustCat = new SelectList(db.CustomerCategories.ToList(), "ID", "CategoryName");
            return View(new ReportViewModel());
        }

        [HttpPost]
        public ActionResult CustomerOutStanding(ReportViewModel model)
        {
            try
            {
                var OSmodel = _manager.GetCustomerOutStanding(model);
                return View("CustomerOutStandingReport", OSmodel);
            }
            catch(Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            ViewBag.CustCat = new SelectList(db.CustomerCategories.ToList(), "ID", "CategoryName");
            return View(model);

        }

        public ActionResult CustomerAccountDetail()
        {
            ViewBag.Customers = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(new ReportViewModel());
        }

        [HttpPost]
        public ActionResult CustomerAccountDetail(ReportViewModel model)
        {
            try
            {
                CustAccDetailRep cusrep = new CustAccDetailRep();

                var report = _manager.GetCustomerAccountDetail(model);
                cusrep.accList = report;
                cusrep.totalOutstanding = db.CustomerAccounts.FirstOrDefault(c => c.CustomerID == model.CustomerID).OutStandingAmount;

                return View("CustomerAccountDetailReport", cusrep);
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Customers = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(model);
        }

        public ActionResult BuyerAccountDetail()
        {
            ViewBag.Buyers = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(new ReportViewModel());
        }

        [HttpPost]
        public ActionResult BuyerAccountDetail(ReportViewModel model)
        {
            try
            {
                BuyerAccDetailRep buyrep = new BuyerAccDetailRep();

                var report = _manager.GetBuyerAccountDetail(model);
                buyrep.accList = report;
                buyrep.totalOutstanding = db.BuyerAccounts.FirstOrDefault(c => c.BuyerID == model.BuyerID).OutStandingAmount;

                return View("BuyerAccountDetailReport", buyrep);
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Buyers = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View(model);
        }

        public ActionResult SalesPerDay()
        {
            return View(new ReportViewModel());
        }

        [HttpPost]
        public ActionResult SalesPerDay(ReportViewModel model)
        {
            try
            {
                var report = _manager.GetSalePerDay(model);
                return View("SalesPerDayReport", report);
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
              return View(model);
        }
    }
}