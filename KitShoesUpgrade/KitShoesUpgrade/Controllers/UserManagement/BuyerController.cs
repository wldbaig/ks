using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Managers;
using KitShoesUpgrade.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.UserManagement
{
    public class BuyerController : BaseController
    {
        BuyerManager _manager;
        public BuyerController()
        {
            _manager = new BuyerManager();
        }
        //
        // GET: /Buyer/
        [RoleSecurity(Permissions.Buyer, PermissionType.VIEW)]
        public ActionResult Index(string str = "")
        {
            ViewBag.Ser = str;
            if (str == "")
            {
                return View(db.Buyers.ToList());
            }
            else
            {
                return View(db.Buyers.Where(c => c.Name.Contains(str)).ToList());
            }
        }

        [HttpPost]
        public ActionResult Search(FormCollection coll)
        {
            var str1 = coll["Search"].ToString();

            if (str1 != null)
            {
                return RedirectToAction("Index", new { str = str1 });
            }
            else
            {
                return RedirectToAction("Index");
            }       
        }

        [RoleSecurity(Permissions.Buyer, PermissionType.ADD)]
        public ActionResult Add()
        {                                                                                        
            return View();
        }

        [HttpPost]
        public ActionResult Add(Buyer model)
        {
            try
            {
                if (ModelState.IsValid && ValidationModel.ValidateBuyer(model, ModelState))
                {
                    _manager.Add(model, User);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {        
                TempData["ERROR"] = ex.Message;
            }

             return View(model);
        }

        [RoleSecurity(Permissions.Buyer, PermissionType.EDIT)]
        public ActionResult Edit(int id)
        {
            var cust = db.Buyers.Find(id);
            if (cust == null)
                return HttpNotFound();

            return View(cust);
        }

        [HttpPost]
        public ActionResult Edit(Buyer model)
        {
            try
            {
                if (ModelState.IsValid && ValidationModel.ValidateBuyer(model, ModelState))
                {
                    _manager.Update(model, User);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {

                TempData["ERROR"] = ex.Message;
            }
            var cust = db.Buyers.Find(model.ID);
            return View(cust);
        }

        public ActionResult ChangeStatus(int id)
        {
            var cust = db.Buyers.Find(id);
            if (cust == null)
                return HttpNotFound();

            cust.IsActive = (cust.IsActive) ? false : true;
            db.Entry(cust).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //[RoleSecurity(Permissions.BuyerAccount, PermissionType.ADD)]
        //public ActionResult RecieveAmount(int id)
        //{
        //    var custAccont = db.BuyerAccounts.Where(m => m.BuyerID == id).FirstOrDefault();
        //    if (custAccont == null)
        //        return HttpNotFound();

        //    return View(custAccont);
        //}

        //[HttpPost]
        //public ActionResult RecieveAmount(BuyerAccount model, FormCollection col)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var amount = Convert.ToDecimal(col["amount"]);
        //            _manager.AddAccount(model, -amount, User);

        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ERROR"] = ex.Message;
        //    }

        //    var byrAccont = db.BuyerAccounts.Find(model.ID);
        //    return View(byrAccont);
        //}

        [RoleSecurity(Permissions.BuyerAccount, PermissionType.ADD)]
        public ActionResult PaidAmount(int id)
        {
            var byrAccont = db.BuyerAccounts.FirstOrDefault(m => m.BuyerID == id);
            if (byrAccont == null)
                return HttpNotFound();

            return View(byrAccont);
        }

        [HttpPost]
        public ActionResult PaidAmount(BuyerAccount model, FormCollection col)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var amount = Convert.ToDecimal(col["amount"]);
                    _manager.AddAccount(model, amount, User);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            var byrAccont = db.BuyerAccounts.Find(model.ID);
            return View(byrAccont);  
        }    
    }
}