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
            
            return View(new Dates());
        }

        [HttpPost]
        public ActionResult BPSale(Dates model)
        {
            ViewBag.StartDate = model.StartDate.Date;
            ViewBag.EndDate = model.EndDate.Date;

            try
            {

                var model1 = _manager.GetBPSaleReport(model);
                return View("BPSaleDetailReport", model1.OrderByDescending(c=>c.Pairs).ToList());

            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }
           
            return View(model);

        }

        [RoleSecurity(Permissions.Report, PermissionType.VIEW)]
        public ActionResult CMSale()
        {

            return View(new Dates());
        }

        [HttpPost]
        public ActionResult CMSale(Dates model)
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

            return View(model);

        }
    }
}