using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using KitShoesUpgrade.Models;
using KitShoesUpgrade.Classes;
using System.Data.Entity;
using System.Data;


namespace KitShoesUpgrade.Controllers.Invoices
{
    public class InvoiceController : BaseController
    {
        //
        // GET: /Invoice/
        [RoleSecurity(Permissions.Invoice, PermissionType.VIEW)]
        public ActionResult Index()
        {
            ViewBag.Date = DateTime.Now.Date.ToShortDateString();

            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.Invoices.OrderByDescending(c => c.AddedOn).ToList().Select(c => new InvoiceViewM()
            {
                InvoiceID = c.InvoiceID,
                AddedOn = c.AddedOn.Date.ToShortDateString(),
                CashCustomer = c.CustomerName,
                CreditCustomer = c.Customer?.Name,
                CustomerType = c.CustomerType,
                TotalPrice = c.TotalPrice

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [RoleSecurity(Permissions.Invoice, PermissionType.ADD)]
        public ActionResult Add()
        {
            ViewBag.CustomerList = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name");

            List<SelectListItem> CustomerType = new List<SelectListItem>();
            CustomerType.Add(new SelectListItem() { Text = "CREDIT", Value = "CREDIT" });
            CustomerType.Add(new SelectListItem() { Text = "CASH", Value = "CASH" });

            ViewBag.Custype = new SelectList(CustomerType, "Value", "Text");

            return View(new InvoiceViewModel());
        }

        [HttpPost]
        public ActionResult Add(InvoiceViewModel model, FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var invoice = new Invoice();

                    invoice.CustomerType = model.CustomerType;
                    if (model.CustomerType == "CASH")
                    {
                        if (string.IsNullOrEmpty(model.CashCustomerName))
                        {
                            throw new Exception("Customer name is required if customer type is CASH");
                        }
                        else
                        {
                            invoice.CustomerName = model.CashCustomerName;
                        }
                    }
                    else
                    {
                        if (model.CreditCustomerID == 0)
                        {
                            throw new Exception("Customer is required if customer type is CREDIT");
                        }
                        else
                        {
                            invoice.CustomerID = model.CreditCustomerID;
                        }

                    }
                    invoice.AmountRecieved = 0;
                    invoice.TotalPrice = 0;
                    invoice.DiscountAmount = 0;
                    invoice.FreightAmount = 0;
                    invoice.CreatedBy = User.ID;
                    invoice.AddedOn = DateTime.UtcNow;

                    db.Invoices.Add(invoice);
                    db.SaveChanges();

                    List<string> keys = new List<string>();

                    for (int i = 1; i < collection.Count; i++)
                    {
                        keys.Add(collection.GetKey(i));
                    }

                    foreach (var item in keys)
                    {
                        if (item.Contains("item-color-ID*"))
                        {

                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.ArticleDetails.Find(iDetails);

                            artDet.Pairs += -Convert.ToInt32(collection[item]);
                            artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + artDet.Pairs;
                            artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                            artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;

                            if (artDet.TotalStock < 0)
                            {
                                throw new Exception("Article " + artDet.Article.ArticleName + " is out of stock");
                            }

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = db.InvoiceDetails.FirstOrDefault(c => c.InvoiceID == invoice.InvoiceID && c.ArticleDetailID == artDet.ID);
                            if (invDet != null)
                            {
                                invDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                invDet.Price = invDet.Price + Convert.ToDecimal(invDet.ArticlePairs * artDet.Article.Price);
                                db.Entry(invDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                invDet = new InvoiceDetail();
                                invDet.InvoiceID = invoice.InvoiceID;
                                invDet.ArticleID = artDet.ArticleId;
                                invDet.ArticleDetailID = artDet.ID;
                                invDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                invDet.ArticleCartons = 0;
                                invDet.Price = Convert.ToDecimal(invDet.ArticlePairs * artDet.Article.Price);
                                db.InvoiceDetails.Add(invDet);
                                db.SaveChanges();
                            }

                        }
                        else if (item.Contains("item-carton-color-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.ArticleDetails.Find(iDetails);

                            artDet.Carton += -Convert.ToInt32(collection[item]);
                            artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + artDet.Pairs;
                            artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                            artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;

                            if (artDet.TotalStock < 0)
                            {
                                throw new Exception("Article " + artDet.Article.ArticleName + " is out of stock");
                            }

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = db.InvoiceDetails.FirstOrDefault(c => c.InvoiceID == invoice.InvoiceID && c.ArticleDetailID == artDet.ID);

                            if (invDet != null)
                            {
                                invDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                invDet.Price = invDet.Price + Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.Entry(invDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                invDet = new InvoiceDetail();
                                invDet.InvoiceID = invoice.InvoiceID;
                                invDet.ArticleID = artDet.ArticleId;
                                invDet.ArticleDetailID = artDet.ID;
                                invDet.ArticlePairs = 0;
                                invDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                invDet.Price = Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.InvoiceDetails.Add(invDet);
                                db.SaveChanges();
                            }
                        }
                    }

                    if (invoice.CustomerType == "CREDIT")
                    {
                        invoice.AmountRecieved = model.AmountPaid;
                    }
                    else
                    {
                        invoice.AmountRecieved = db.InvoiceDetails.Where(c => c.InvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    }

                    invoice.TotalPrice = db.InvoiceDetails.Where(c => c.InvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    invoice.DiscountAmount = model.DiscountAmount;
                    invoice.FreightAmount = model.Freight;
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    if (invoice.CustomerType == "CREDIT")
                    {
                        var custAccount = new CustomerAccountDetail();
                        custAccount.CAccountID = db.Customers.Find(invoice.CustomerID).CustomerAccounts.FirstOrDefault().ID;
                        custAccount.CreatedBy = User.ID;
                        custAccount.CreatedOn = DateTime.UtcNow;
                        custAccount.InvoiceID = invoice.InvoiceID;
                        custAccount.TotalAmount = model.AmountPaid;
                        custAccount.Description = "INVOICE";
                        db.CustomerAccountDetails.Add(custAccount);
                        db.SaveChanges();

                        var CAcount = invoice.Customer.CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                        CAcount.TotalBalance += (invoice.TotalPrice - invoice.DiscountAmount + invoice.FreightAmount);
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            ViewBag.CustomerList = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name");

            List<SelectListItem> CustomerType = new List<SelectListItem>();
            CustomerType.Add(new SelectListItem() { Text = "CREDIT", Value = "CREDIT" });
            CustomerType.Add(new SelectListItem() { Text = "CASH", Value = "CASH" });

            ViewBag.Custype = new SelectList(CustomerType, "Value", "Text");

            return View(model);
        }

        public ActionResult PurchaseReciept(int id)
        {
            var invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();
            if (invoice.CustomerID != null)
            {
                Pmodel.CustomerInfo = db.Customers.Find(invoice.CustomerID);
                Pmodel.PreviousOutStanding = Pmodel.CustomerInfo.CustomerAccounts.FirstOrDefault().PreviousOutStanding;
            }
            else
            {
                Pmodel.CustomerInfo = new Customer();
                Pmodel.CustomerInfo.Name = invoice.CustomerName;
                Pmodel.PreviousOutStanding = 0;
            }
            Pmodel.Date = invoice.AddedOn.ToShortDateString();
            Pmodel.InvoiceID = invoice.InvoiceID;
            Pmodel.AmountRecieved = invoice.AmountRecieved;
            Pmodel.Discount = invoice.DiscountAmount;
            Pmodel.Freight = invoice.FreightAmount;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in invoice.InvoiceDetails)
            {
                if (!(ITEM.ArticlePairs == 0 && ITEM.ArticleCartons == 0))
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.ArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = (int)ITEM.ArticlePairs;
                        prD.CartonAdded = (int)ITEM.ArticleCartons;
                        prD.Price = ITEM.Price;

                        pr.LPRDetail.Add(prD);

                    }
                    else
                    {
                        pr = new PurchaseReciept();
                        pr.Article = ITEM.ArticleID;
                        pr.UnitPrice = ITEM.Article.Price;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = (int)ITEM.ArticlePairs;
                        prD.CartonAdded = (int)ITEM.ArticleCartons;
                        prD.Price = ITEM.Price;

                        pr.LPRDetail.Add(prD);

                        Pmodel.PReciept.Add(pr);
                    }
                }

            }

            return View(Pmodel);
        }

        public ActionResult CancilInvoice([DataSourceRequest] DataSourceRequest request, InvoiceViewM model)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var inv = db.Invoices.Find(model.InvoiceID);

                    if (inv == null)
                        return HttpNotFound();

                    foreach (var item in inv.InvoiceDetails)
                    {
                        var artDetId = item.ArticleDetailID;
                        var carton = item.ArticleCartons;
                        var pairs = item.ArticlePairs;

                        var det = db.ArticleDetails.Find(artDetId);

                        det.Carton += (item.ArticleCartons ?? 0);
                        det.Pairs += (item.ArticlePairs ?? 0);

                        det.TotalStock = (det.Carton * det.Article.PairInCarton) + det.Pairs;

                        db.Entry(det).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    db.InvoiceDetails.RemoveRange(inv.InvoiceDetails);
                    db.SaveChanges();

                    if (inv.CustomerType == "CREDIT")
                    {
                        var CAcount = inv.Customer.CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid += (-1 * inv.AmountRecieved);
                        CAcount.TotalBalance += (-1 * (inv.TotalPrice - inv.DiscountAmount + inv.FreightAmount));

                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                        var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.InvoiceID == inv.InvoiceID);
                        if (custAccDetail != null)
                        {
                            db.CustomerAccountDetails.Remove(custAccDetail);
                            db.SaveChanges();
                        }
                    }

                    db.Invoices.Remove(inv);
                    db.SaveChanges();

                    trans.Complete();

                    TempData["SUCCESS"] = "Record deleted successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [RoleSecurity(Permissions.Invoice, PermissionType.EDIT)]
        public ActionResult EditInvoice(int id)
        {

            var invoice = db.Invoices.Find(id);
            if (invoice == null)
                return HttpNotFound();
            EditInvoiceViewModel model = new EditInvoiceViewModel();

            model.Invoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.Article = invoice.InvoiceDetails.Select(c => c.Article).Distinct().ToList();

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            if (invoice.CustomerID != null)
            {
                ViewBag.CustomerList = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name", invoice.CustomerID);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditInvoice(EditInvoiceViewModel model, FormCollection collection)
        {
            var invoice = db.Invoices.Find(model.InvoiceID);
            var DiscountAmount = Convert.ToDecimal(collection["Discount"]);
            var Freight = Convert.ToDecimal(collection["Freight"]);
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in invoice.InvoiceDetails)
                    {
                        ArticleDetail artDet = db.ArticleDetails.Find(item.ArticleDetailID);

                        artDet.Pairs += item.ArticlePairs ?? 0;
                        artDet.Carton += item.ArticleCartons ?? 0;
                        artDet.TotalStock = (artDet.Carton * item.Article.PairInCarton) + artDet.Pairs;

                        db.Entry(artDet).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    if (invoice.CustomerType == "CREDIT")
                    {

                        var CAcount = invoice.Customer.CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid += (-1 * invoice.AmountRecieved);
                        CAcount.TotalBalance += (-1 * invoice.TotalPrice);
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                        var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.InvoiceID == invoice.InvoiceID);
                        if (custAccDetail != null)
                        {
                            db.CustomerAccountDetails.Remove(custAccDetail);
                            db.SaveChanges();
                        }

                    }

                    db.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                    db.SaveChanges();

                    List<string> keys = new List<string>();

                    for (int i = 1; i < collection.Count; i++)
                    {
                        keys.Add(collection.GetKey(i));
                    }

                    foreach (var item in keys)
                    {
                        if (item.Contains("item-color-ID*"))
                        {

                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.ArticleDetails.Find(iDetails);

                            artDet.Pairs += -Convert.ToInt32(collection[item]);
                            artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + artDet.Pairs;
                            artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                            artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;

                            if (artDet.TotalStock < 0)
                            {
                                throw new Exception("Article " + artDet.Article.ArticleName + " is out of stock");
                            }

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = db.InvoiceDetails.FirstOrDefault(c => c.InvoiceID == invoice.InvoiceID && c.ArticleDetailID == artDet.ID);
                            if (invDet != null)
                            {
                                invDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                invDet.Price = invDet.Price + Convert.ToDecimal(invDet.ArticlePairs * artDet.Article.Price);
                                db.Entry(invDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                invDet = new InvoiceDetail();
                                invDet.InvoiceID = invoice.InvoiceID;
                                invDet.ArticleID = artDet.ArticleId;
                                invDet.ArticleDetailID = artDet.ID;
                                invDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                invDet.ArticleCartons = 0;
                                invDet.Price = Convert.ToDecimal(invDet.ArticlePairs * artDet.Article.Price);
                                db.InvoiceDetails.Add(invDet);
                                db.SaveChanges();
                            }

                        }
                        else if (item.Contains("item-carton-color-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.ArticleDetails.Find(iDetails);

                            artDet.Carton += -Convert.ToInt32(collection[item]);
                            artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + artDet.Pairs;
                            artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                            artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;

                            if (artDet.TotalStock < 0)
                            {
                                throw new Exception("Article " + artDet.Article.ArticleName + " is out of stock");
                            }

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = db.InvoiceDetails.FirstOrDefault(c => c.InvoiceID == invoice.InvoiceID && c.ArticleDetailID == artDet.ID);

                            if (invDet != null)
                            {
                                invDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                invDet.Price = invDet.Price + Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.Entry(invDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                invDet = new InvoiceDetail();
                                invDet.InvoiceID = invoice.InvoiceID;
                                invDet.ArticleID = artDet.ArticleId;
                                invDet.ArticleDetailID = artDet.ID;
                                invDet.ArticlePairs = 0;
                                invDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                invDet.Price = Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.InvoiceDetails.Add(invDet);
                                db.SaveChanges();
                            }
                        }
                    }

                    if (invoice.CustomerType == "CREDIT")
                    {
                        invoice.AmountRecieved = Convert.ToDecimal(collection["AmountPaid"]);
                        invoice.CustomerID = model.Invoice.CustomerID;
                    }
                    else
                    {
                        invoice.AmountRecieved = db.InvoiceDetails.Where(c => c.InvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    }

                    invoice.TotalPrice = db.InvoiceDetails.Where(c => c.InvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    invoice.DiscountAmount = DiscountAmount;
                    invoice.FreightAmount = Freight;
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    if (invoice.CustomerType == "CREDIT")
                    {

                        var custAccount = new CustomerAccountDetail();
                        custAccount.CAccountID = db.Customers.Find(invoice.CustomerID).CustomerAccounts.FirstOrDefault().ID;
                        custAccount.CreatedBy = User.ID;
                        custAccount.CreatedOn = DateTime.UtcNow;
                        custAccount.InvoiceID = invoice.InvoiceID;
                        custAccount.TotalAmount = Convert.ToDecimal(collection["AmountPaid"]); // model.AmountPaid;
                        custAccount.Description = "INVOICE";
                        db.CustomerAccountDetails.Add(custAccount);
                        db.SaveChanges();

                        var CAcount = invoice.Customer.CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                        CAcount.TotalBalance += (invoice.TotalPrice - invoice.DiscountAmount + invoice.FreightAmount);
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            model.Invoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.Article = invoice.InvoiceDetails.Select(c => c.Article).Distinct().ToList();

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            if (invoice.CustomerID != null)
            {
                ViewBag.CustomerList = new SelectList(db.Customers.Where(c => c.IsActive == true).ToList(), "ID", "Name", invoice.CustomerID);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult GetArticleDetailforInvoice(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.Articles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> <div class='form-group'>";
            result = result + "<input type='hidden' id='item-price-" + artDet.ID + "' value ='" + artDet.Price + "' /></div>";
            result = result += "<div class='form-group'> <div class='col-sm-3'  style='width:33.33%'>Color</div> <div class='col-sm-3'  style='width:33.33%'>Carton</div> <div class='col-sm-3'  style='width:33.33%'>Pair</div> </div>";
            foreach (var item in artDet.ArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";//<div class='col-sm-3'>Avlailable(" + item.TotalStock + ")</div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'>" + item.Carton + "<input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.Carton + " onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'>" + item.Pairs + "<input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div></div>";
                result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCustomerRemainingBalance(int CustomerID)
        {
            if (CustomerID == 0)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var cus = db.Customers.Find(CustomerID);

            return Json(cus.CustomerAccounts.FirstOrDefault().OutStandingAmount, JsonRequestBehavior.AllowGet);
        }

        public decimal GetCustomerBalance(int CustomerID)
        {
            if (CustomerID == 0)
            {
                return 0;
            }
            var cus = db.Customers.Find(CustomerID);

            return cus.CustomerAccounts.FirstOrDefault().OutStandingAmount;
        }

        public ActionResult GetArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.ArticleDetails.Where(c => c.TotalStock > 0 && c.Article.IsActive == true).Select(c => c.Article).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}