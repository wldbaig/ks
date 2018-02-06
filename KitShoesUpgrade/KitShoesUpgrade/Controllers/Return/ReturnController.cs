using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.Return
{
    public class ReturnController : BaseController
    {
        // GET: Return
        [RoleSecurity(Permissions.Return, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.ReturnItems.OrderByDescending(c => c.AddedOn).ToList().Select(c => new ReturnItemM()
            {
                ReturnItemID = c.ReturnItemID,

                AddedOn = c.AddedOn.ToString("dd-MMM-yyyy"),
                CustomerName = (c.CustomerID == null) ? c.CustomerName : db.Customers.FirstOrDefault(x => x.ID == c.CustomerID).Name,
                CustomerType = c.CustomerType,
                Price = c.TotalPrice
            }).ToList();
            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailRead([DataSourceRequest] DataSourceRequest request, int ReturnItem)
        {
            var x = db.ReturnItemDetails.Where(c => c.ReturnItemID == ReturnItem).Select(c => new ReturnItemDetM()
            {
                ID = c.ID,
                ReturnItemID = c.ReturnItemID,
                Price = c.Price,
                ArticleDetailID = c.ArticleDetailID,
                ArticleID = c.ArticleID,
                ArticleName = c.Article.ArticleName,
                Cartons = c.ArticleCartons,
                ColorName = c.ArticleDetail.Color.ColorName,
                Pairs = c.ArticlePairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [RoleSecurity(Permissions.Return, PermissionType.ADD)]
        public ActionResult Add()
        {

            //ViewBag.ArticleList = new SelectList(db.ArticleDetails.Where(c => c.TotalStock != 0).Select(c => c.Article).Distinct().ToList(), "ID", "ArticleName");
            ViewBag.CustomerList = new SelectList(db.Customers.Where(c=>c.IsActive == true).ToList(), "ID", "Name");

            List<SelectListItem> CustomerType = new List<SelectListItem>();
            CustomerType.Add(new SelectListItem() { Text = "CREDIT", Value = "CREDIT" });
            CustomerType.Add(new SelectListItem() { Text = "CASH", Value = "CASH" });

            ViewBag.Custype = new SelectList(CustomerType, "Value", "Text");

            return View(new ReturnViewModel());
        }

        [HttpPost]
        public ActionResult Add(ReturnViewModel model, FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var returnitems = new ReturnItem();

                    returnitems.CustomerType = model.CustomerType;
                    if (model.CustomerType == "CASH")
                    {
                        if (string.IsNullOrEmpty(model.CashCustomerName))
                        {
                            throw new Exception("Customer name is required if customer type is CASH");
                        }
                        else
                        {
                            returnitems.CustomerName = model.CashCustomerName;
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
                            returnitems.CustomerID = model.CreditCustomerID;
                        }

                    }
                    returnitems.Claim = model.Claim;
                    returnitems.TotalPrice = 0;
                    returnitems.CreatedBy = User.ID;
                    returnitems.AddedOn = DateTime.UtcNow;

                    db.ReturnItems.Add(returnitems);
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

                            artDet.Pairs += Convert.ToInt32(collection[item]);
                            artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + artDet.Pairs;
                            artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                            artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;
                             
                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var retDet = db.ReturnItemDetails.FirstOrDefault(c => c.ReturnItemID == returnitems.ReturnItemID && c.ArticleDetailID == artDet.ID);
                            if (retDet != null)
                            {
                                retDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                retDet.Price = retDet.Price + Convert.ToDecimal(retDet.ArticlePairs * artDet.Article.Price);
                                db.Entry(retDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                retDet = new ReturnItemDetail();
                                retDet.ReturnItemID = returnitems.ReturnItemID;
                                retDet.ArticleID = artDet.ArticleId;
                                retDet.ArticleDetailID = artDet.ID;
                                retDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                retDet.ArticleCartons = 0;
                                retDet.Price = Convert.ToDecimal(retDet.ArticlePairs * artDet.Article.Price);
                                db.ReturnItemDetails.Add(retDet);
                                db.SaveChanges();
                            }

                        }
                        else if (item.Contains("item-carton-color-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.ArticleDetails.Find(iDetails);

                            artDet.Carton += Convert.ToInt32(collection[item]);
                            artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + artDet.Pairs;
                            artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                            artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;

                            if (artDet.TotalStock < 0)
                            {
                                throw new Exception("Article " + artDet.Article.ArticleName + " is out of stock");
                            }

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var retDet = db.ReturnItemDetails.FirstOrDefault(c => c.ReturnItemID == returnitems.ReturnItemID && c.ArticleDetailID == artDet.ID);

                            if (retDet != null)
                            {
                                retDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                retDet.Price = retDet.Price + Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.Entry(retDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                retDet = new ReturnItemDetail();
                                retDet.ReturnItemID = returnitems.ReturnItemID;
                                retDet.ArticleID = artDet.ArticleId;
                                retDet.ArticleDetailID = artDet.ID;
                                retDet.ArticlePairs = 0;
                                retDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                retDet.Price = Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.ReturnItemDetails.Add(retDet);
                                db.SaveChanges();
                            }
                        }
                    }

                    returnitems.TotalPrice = db.ReturnItemDetails.Where(c => c.ReturnItemID == returnitems.ReturnItemID).Sum(c => c.Price);
                    db.Entry(returnitems).State = EntityState.Modified;
                    db.SaveChanges();

                    if (returnitems.CustomerType == "CREDIT")
                    {

                        var custAccount = new CustomerAccountDetail();
                        custAccount.CAccountID = db.Customers.Find(returnitems.CustomerID).CustomerAccounts.FirstOrDefault().ID;
                        custAccount.CreatedBy = User.ID;
                        custAccount.CreatedOn = DateTime.UtcNow;
                        custAccount.ReturnID = returnitems.ReturnItemID;
                        custAccount.TotalAmount = (decimal)(returnitems.TotalPrice + returnitems.Claim);
                        custAccount.Description = "RETURN";
                        db.CustomerAccountDetails.Add(custAccount);
                        db.SaveChanges();

                        var CAcount = returnitems.Customer.CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount); 
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                        custAccount.PreviousOutStanding = CAcount.PreviousOutStanding;
                        db.Entry(custAccount).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    trans.Complete();
                    TempData["SUCCESS"] = "Return added successfully";

                    return RedirectToAction("ReturnReciept", new { id = returnitems.ReturnItemID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

              ViewBag.CustomerList = new SelectList(db.Customers.Where(c=>c.IsActive==true).ToList(), "ID", "Name");

            List<SelectListItem> CustomerType = new List<SelectListItem>();
            CustomerType.Add(new SelectListItem() { Text = "CREDIT", Value = "CREDIT" });
            CustomerType.Add(new SelectListItem() { Text = "CASH", Value = "CASH" });

            ViewBag.Custype = new SelectList(CustomerType, "Value", "Text");

            return View(model);
        }

        [HttpPost]
        public ActionResult GetArticleDetailforReturn(string ArticleID)
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
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0'   onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div></div>";
                result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnReciept(int id)
        {
            var ret = db.ReturnItems.Find(id);
            if (ret == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();
            if (ret.CustomerID != null)
            {
                Pmodel.CustomerInfo = db.Customers.Find(ret.CustomerID);
            }
            else
            {
                Pmodel.CustomerInfo = new Customer();
                Pmodel.CustomerInfo.Name = ret.CustomerName;
            }
            Pmodel.ReturnID = ret.ReturnItemID;
            Pmodel.Discount = (decimal)ret.Claim;
            Pmodel.PReciept = new List<PurchaseReciept>();
            Pmodel.Date = ret.AddedOn.Date.ToShortDateString();
            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in ret.ReturnItemDetails)
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

        public ActionResult GetArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.ArticleDetails.Where(c =>  c.Article.IsActive == true).Select(c => c.Article).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, ReturnItemM model)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var ret = db.ReturnItems.Find(model.ReturnItemID);

                    if (ret == null)
                        return HttpNotFound();

                    foreach (var item in ret.ReturnItemDetails)
                    {
                        var artDetId = item.ArticleDetailID;
                        var carton = item.ArticleCartons;
                        var pairs = item.ArticlePairs;

                        var det = db.ArticleDetails.Find(artDetId);

                        det.Carton -= (item.ArticleCartons ?? 0);
                        det.Pairs -= (item.ArticlePairs ?? 0);

                        det.TotalStock = (det.Carton * det.Article.PairInCarton) + det.Pairs;

                        db.Entry(det).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    db.ReturnItemDetails.RemoveRange(ret.ReturnItemDetails);
                    db.SaveChanges();

                    if (ret.CustomerType == "CREDIT")
                    {
                        var CAcount = ret.Customer.CustomerAccounts.FirstOrDefault();
                        CAcount.TotalPaid += (-1 * (decimal)(ret.TotalPrice + ret.Claim));
                        CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                        CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                        CAcount.UpdatedOn = DateTime.UtcNow;
                        CAcount.UpdatedBy = User.ID;
                        db.Entry(CAcount).State = EntityState.Modified;
                        db.SaveChanges();

                        var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.ReturnID == ret.ReturnItemID);
                        if (custAccDetail != null)
                        {
                            db.CustomerAccountDetails.Remove(custAccDetail);
                            db.SaveChanges();
                        }
                    }

                    db.ReturnItems.Remove(ret);
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
         
    }
}