using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Managers;
using KitShoesUpgrade.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.UserManagement
{
    public class CustomerController : BaseController
    {
        CustomerManager _manager;
        public CustomerController()
        {
            _manager = new CustomerManager();
        }
        //
        // GET: /Customer/
        [RoleSecurity(Permissions.Customer, PermissionType.VIEW)]
        public ActionResult Index(string str = "")
        {
            ViewBag.Ser = str;
            if (str == "")
            {
                return View(db.Customers.ToList());
            }
            else
            {
               return View(db.Customers.Where(c => c.Name.Contains(str)).ToList());
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

        [RoleSecurity(Permissions.Customer, PermissionType.ADD)]
        public ActionResult Add()
        {
            ViewBag.CategoryList = new SelectList(db.CustomerCategories, "ID", "CategoryName");
            return View();
        }

        [HttpPost]
        public ActionResult Add(Customer model)
        {
            try
            {
                if (ModelState.IsValid && ValidationModel.ValidateCustomer(model, ModelState))
                {
                    _manager.Add(model, User);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {

                TempData["ERROR"] = ex.Message;
            }

            ViewBag.CategoryList = new SelectList(db.CustomerCategories, "ID", "CategoryName");
            return View(model);
        }

        [RoleSecurity(Permissions.Customer, PermissionType.EDIT)]
        public ActionResult Edit(int id)
        {
            var cust = db.Customers.Find(id);
            if (cust == null)
                return HttpNotFound();

            ViewBag.CategoryList = new SelectList(db.CustomerCategories, "ID", "CategoryName");
            return View(cust);
        }

        [HttpPost]
        public ActionResult Edit(Customer model)
        {
            try
            {
                if (ModelState.IsValid && ValidationModel.ValidateCustomer(model, ModelState))
                {
                    _manager.Update(model, User);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {

                TempData["ERROR"] = ex.Message;
            }
            var cust = db.Customers.Find(model.ID);
            ViewBag.CategoryList = new SelectList(db.CustomerCategories, "ID", "CategoryName");
            return View(cust);
        }

        public ActionResult ChangeStatus(int id)
        {
            var cust = db.Customers.Find(id);
            if (cust == null)
                return HttpNotFound();

            cust.IsActive = (cust.IsActive) ? false : true;
            db.Entry(cust).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [RoleSecurity(Permissions.CustomerAccount, PermissionType.ADD)]
        public ActionResult RecieveAmount(int id)
        {
            var custAccont = db.CustomerAccounts.Where(m => m.CustomerID == id).FirstOrDefault();
            if (custAccont == null)
                return HttpNotFound();

            return View(custAccont);
        }

        [HttpPost]
        public ActionResult RecieveAmount(CustomerAccount model, FormCollection col)
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

            var custAccont = db.CustomerAccounts.Find(model.ID);
            return View(custAccont);
        }

        [RoleSecurity(Permissions.CustomerAccount, PermissionType.ADD)]
        public ActionResult PaidAmount(int id)
        {
            var custAccont = db.CustomerAccounts.Where(m => m.CustomerID == id).FirstOrDefault();
            if (custAccont == null)
                return HttpNotFound();

            return View(custAccont);
        }

        [HttpPost]
        public ActionResult PaidAmount(CustomerAccount model, FormCollection col)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var amount = Convert.ToDecimal(col["amount"]);
                    _manager.AddAccount(model, -amount, User);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {                
                TempData["ERROR"] = ex.Message;
            }

            var custAccont = db.CustomerAccounts.Find(model.ID);
            return View(custAccont);

        }

    }
}