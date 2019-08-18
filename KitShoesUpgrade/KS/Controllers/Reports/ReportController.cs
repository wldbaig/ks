using KS.Classes;
using KS.Managers;
using KS.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace KS.Controllers.ReportManagement
{
    public class ReportController : BaseController
    {
        ReportManager _manager;

        public ReportController()
        {
            _manager = new ReportManager();
        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult BPSale()
        {
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(new SaleModel());
        }

        [HttpPost]
        public ActionResult BPSale(SaleModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {
                var model1 = _manager.GetBPSaleReport(model);
                return View("BPSaleDetailReport", model1.OrderByDescending(c => c.Pairs).ToList());
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(model);
        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult CMSale()
        {
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(new SaleModel());
        }

        [HttpPost]
        public ActionResult CMSale(SaleModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {
                var model1 = _manager.GetCMSaleReport(model);
                return View("CMSaleDetailReport", model1.OrderByDescending(c => c.Pairs).ToList());
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(model);

        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult TSSale()
        {
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(new SaleModel());
        }

        [HttpPost]
        public ActionResult TSSale(SaleModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {
                var model1 = _manager.GetTSSaleReport(model);
                return View("TSSaleDetailReport", model1.OrderByDescending(c => c.Pairs).ToList());
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(model);

        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult JPSale()
        {
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(new SaleModel());
        }

        [HttpPost]
        public ActionResult JPSale(SaleModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {
                var model1 = _manager.GetJPSaleReport(model);
                return View("JPSaleDetailReport", model1.OrderByDescending(c => c.Pairs).ToList());
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Category = new SelectList(db.Categories.ToList(), "ID", "CategoryName");
            return View(model);

        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult ArticleSaleWithCreationDates()
        {
            ViewBag.Shop = new SelectList(db.Shops.Where(c => c.NickName != "WH").ToList(), "ID", "ShopName");
            return View(new SaleModel());
        }

        [HttpPost]
        public ActionResult ArticleSaleWithCreationDates(SaleModel model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {
                var model1 = _manager.GetArticleSaleReport(model);
                return View("ArticleSaleReport", model1.OrderByDescending(c => c.Pairs).ToList());
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
            ViewBag.Shop = new SelectList(db.Shops.Where(c => c.NickName != "WH").ToList(), "ID", "ShopName");

            return View(model);

        }

    }
}