using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.CR
{
    public class CashRecieptController : BaseController
    {
        // GET: CashReciept
        [RoleSecurity(Permissions.CashReciept, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.CashReciepts.OrderByDescending(c => c.AddedOn).ToList().Select(c => new CashRecViewM()
            {
                CashRecID = c.CashRecieptID,
                Type = c.CashType,
                Date = c.AddedOn.Date.ToShortDateString(),
                TotalAmount = c.TotalAmount

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailRead([DataSourceRequest] DataSourceRequest request, int CashRecieptID)
        {
            var x = db.CashRecieptDetails.Where(c => c.CashRecieptID == CashRecieptID).Select(c => new CashRecDetViewM()
            {
                CashRecID = c.CashRecieptID,
                CashRecDetID = c.CashRecieptDetailID,
                Amount = c.Amount,
                CutomerId = c.CustomerID,
                Name = c.Customer.Name
            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDetail([DataSourceRequest] DataSourceRequest request, CashRecDetViewM model)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var cashR = db.CashReciepts.Find(model.CashRecID);
                    var cashRecDet = db.CashRecieptDetails.FirstOrDefault(c => c.CashRecieptDetailID == model.CashRecDetID);
                    if (cashRecDet.CashReciept.CashType == "PAY AMOUNT")
                    {
                        var CAcount = db.Customers.Find(model.CutomerId).CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid += (cashRecDet.Amount);
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                        var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.CashRecieptID == model.CashRecID);
                        if (custAccDetail != null)
                        {
                            db.CustomerAccountDetails.Remove(custAccDetail);
                            db.SaveChanges();
                        }


                        cashRecDet.Amount = model.Amount;
                        db.Entry(cashRecDet).State = EntityState.Modified;
                        db.SaveChanges();


                        var custAccount = new CustomerAccountDetail();
                        custAccount.CAccountID = db.Customers.Find(model.CutomerId).CustomerAccounts.FirstOrDefault().ID;
                        custAccount.CreatedBy = User.ID;
                        custAccount.CreatedOn = DateTime.UtcNow;
                        custAccount.TotalAmount = -model.Amount;
                        custAccount.CashRecieptID = model.CashRecID;
                        custAccount.Description = "CASH PAID";
                        db.CustomerAccountDetails.Add(custAccount);
                        db.SaveChanges();


                        CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();


                        cashR.TotalAmount = cashR.CashRecieptDetails.Sum(c => c.Amount);
                        db.Entry(cashR).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {

                        var CAcount = db.Customers.Find(model.CutomerId).CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid += (-1 * cashRecDet.Amount);
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                        var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.CashRecieptID == model.CashRecID);
                        if (custAccDetail != null)
                        {
                            db.CustomerAccountDetails.Remove(custAccDetail);
                            db.SaveChanges();
                        }


                        cashRecDet.Amount = model.Amount;
                        db.Entry(cashRecDet).State = EntityState.Modified;
                        db.SaveChanges();


                        var custAccount = new CustomerAccountDetail();
                        custAccount.CAccountID = db.Customers.Find(model.CutomerId).CustomerAccounts.FirstOrDefault().ID;
                        custAccount.CreatedBy = User.ID;
                        custAccount.CreatedOn = DateTime.UtcNow;
                        custAccount.TotalAmount = model.Amount;
                        custAccount.CashRecieptID = model.CashRecID;
                        custAccount.Description = "CASH RECIEPT";
                        db.CustomerAccountDetails.Add(custAccount);
                        db.SaveChanges();


                        CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();


                        cashR.TotalAmount = cashR.CashRecieptDetails.Sum(c => c.Amount);
                        db.Entry(cashR).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    trans.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index");
        }


        [RoleSecurity(Permissions.CashReciept, PermissionType.ADD)]
        public ActionResult Recieve()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Recieve(CashCustViewModel model, FormCollection fm)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var cashRec = new CashReciept();

                    cashRec.AddedOn = DateTime.Now;
                    cashRec.CreatedBy = User.ID;
                    cashRec.CashType = "RECIEVE AMOUNT";
                    cashRec.TotalAmount = 0;

                    db.CashReciepts.Add(cashRec);
                    db.SaveChanges();


                    foreach (var cusId in model.Custs)
                    {
                        var amount = Convert.ToDecimal(fm["item-Cust-ID*" + cusId]);
                        if (amount != 0)
                        {
                            var custAccount = new CustomerAccountDetail();
                            custAccount.CAccountID = db.Customers.Find(cusId).CustomerAccounts.FirstOrDefault().ID;
                            custAccount.CreatedBy = User.ID;
                            custAccount.CreatedOn = DateTime.UtcNow;
                            custAccount.TotalAmount = amount;
                            custAccount.Description = "CASH RECIEPT";
                            custAccount.CashRecieptID = cashRec.CashRecieptID;
                            db.CustomerAccountDetails.Add(custAccount);
                            db.SaveChanges();

                            var CAcount = db.Customers.Find(cusId).CustomerAccounts.FirstOrDefault();
                            CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                            CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                            CAcount.UpdatedOn = DateTime.UtcNow;
                            CAcount.UpdatedBy = User.ID;
                            db.Entry(CAcount).State = EntityState.Modified;
                            db.SaveChanges();


                            var cashRecDet = new CashRecieptDetail();
                            cashRecDet.CashRecieptID = cashRec.CashRecieptID;
                            cashRecDet.CustomerID = cusId;
                            cashRecDet.Amount = amount;
                            db.CashRecieptDetails.Add(cashRecDet);
                            db.SaveChanges();
                        }
                    }
                    cashRec.TotalAmount = cashRec.CashRecieptDetails.Sum(c => c.Amount);
                    db.Entry(cashRec).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "CashReciept added successfully";


                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            return View(model);
        }

        [RoleSecurity(Permissions.CashReciept, PermissionType.ADD)]
        public ActionResult Pay()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Pay(CashCustViewModel model, FormCollection fm)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var cashRec = new CashReciept();

                    cashRec.AddedOn = DateTime.Now;
                    cashRec.CreatedBy = User.ID;
                    cashRec.CashType = "PAY AMOUNT";
                    cashRec.TotalAmount = 0;

                    db.CashReciepts.Add(cashRec);
                    db.SaveChanges();


                    foreach (var cusId in model.Custs)
                    {
                        var amount = Convert.ToDecimal(fm["item-Cust-ID*" + cusId]);
                        if (amount != 0)
                        {
                            var custAccount = new CustomerAccountDetail();
                            custAccount.CAccountID = db.Customers.Find(cusId).CustomerAccounts.FirstOrDefault().ID;
                            custAccount.CreatedBy = User.ID;
                            custAccount.CreatedOn = DateTime.UtcNow;
                            custAccount.TotalAmount = -amount;
                            custAccount.Description = "CASH PAID";

                            custAccount.CashRecieptID = cashRec.CashRecieptID;
                            db.CustomerAccountDetails.Add(custAccount);
                            db.SaveChanges();

                            var CAcount = db.Customers.Find(cusId).CustomerAccounts.FirstOrDefault();
                            CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                            CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                            CAcount.UpdatedOn = DateTime.UtcNow;
                            CAcount.UpdatedBy = User.ID;
                            db.Entry(CAcount).State = EntityState.Modified;
                            db.SaveChanges();


                            var cashRecDet = new CashRecieptDetail();
                            cashRecDet.CashRecieptID = cashRec.CashRecieptID;
                            cashRecDet.CustomerID = cusId;
                            cashRecDet.Amount = amount;
                            db.CashRecieptDetails.Add(cashRecDet);
                            db.SaveChanges();
                        }
                    }
                    cashRec.TotalAmount = cashRec.CashRecieptDetails.Sum(c => c.Amount);
                    db.Entry(cashRec).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "CashReciept added successfully";

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            return View(model);
        }

        [RoleSecurity(Permissions.CashReciept, PermissionType.ADD)]
        public ActionResult BuyerRecieve()
        {
            ViewBag.Buyer = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult BuyerRecieve(FormCollection fm)
        {
            ViewBag.Buyer = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View();
        }

        [RoleSecurity(Permissions.CashReciept, PermissionType.ADD)]
        public ActionResult BuyerPay()
        {
            ViewBag.Buyer = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult BuyerPay(FormCollection fm)
        {
            ViewBag.Buyer = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult GetCustomers(string CustomerNAME)
        {
            if (CustomerNAME == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var cust = db.Customers.FirstOrDefault(c => c.Name == CustomerNAME);
            var CUSTACC = db.CustomerAccounts.FirstOrDefault(C => C.CustomerID == cust.ID).OutStandingAmount;

            var result = "";
            result = result + "<div class='custom' id='" + cust.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + cust.Name + " </b></div>   ";
            result = result + "<div  class='col-sm-3'  >" + CUSTACC + " <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-Cust-" + cust.ID + "' placeholder='Enter Amount' name = 'item-Cust-ID*" + cust.ID + "' value = '0' min = '0' onchange= 'calcuatePrice(" + cust.ID + " )' ></input> </div>";
            result = result + "<div class=' itemPrices' id='cust-" + cust.ID + "' style = 'display:none'>0</div>";
            result = result + "<div class=' itemrPrices-" + cust.ID + "' id='custr-" + cust.ID + "' style = 'display:none'>0</div>";

            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomerList()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.Customers.Where(c => c.IsActive == true).Distinct().ToList();

            var list = result.Select(c => new CustomerList()
            {
                ID = c.ID,
                Name = c.Name
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}