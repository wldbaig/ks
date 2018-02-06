using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.ReserveSales
{
    public class ReserveSaleController : BaseController
    {
        // GET: ReseveSale
        [RoleSecurity(Permissions.ReserveSale, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.ReserveSales.OrderByDescending(c => c.AddedOn).ToList().Select(c => new ReserveViewM()
            {
                ReserveSaleID = c.ReserveSaleID,
                AddedOn = c.AddedOn.Date.ToShortDateString(),
                CreditCustomer = c.Customer.Name,
                TotalPrice = c.TotalPrice

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailRead([DataSourceRequest] DataSourceRequest request, int ReserveSaleID)
        {
            var x = db.ReserveSaleDetails.Where(c => c.ReserveSaleID == ReserveSaleID).Select(c => new ReserveDetViewM()
            {
                ReserveID = c.ReserveSaleID,
                Article = db.Articles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.ArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                ReserveDetailID = c.ID,
                Pair = c.ArticlePairs,
                Carton = c.ArticleCartons

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [RoleSecurity(Permissions.ReserveSale, PermissionType.ADD)]
        public ActionResult Add()
        {
            ViewBag.CustomerList = new SelectList(db.Customers.ToList(), "ID", "Name");
            return View(new ReserveViewModel());
        }

        [HttpPost]
        public ActionResult Add(ReserveViewModel model, FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var reserve = new ReserveSale();


                    if (model.CreditCustomerID == 0)
                    {
                        throw new Exception("Customer is required");
                    }
                    else
                    {
                        reserve.CustomerID = model.CreditCustomerID;
                    }


                    reserve.TotalPrice = 0;
                    reserve.AmountRecieved = 0;
                    reserve.CreatedBy = User.ID;
                    reserve.AddedOn = DateTime.UtcNow;

                    db.ReserveSales.Add(reserve);
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

                            var resDet = db.ReserveSaleDetails.FirstOrDefault(c => c.ReserveSaleID == reserve.ReserveSaleID && c.ArticleDetailID == artDet.ID);
                            if (resDet != null)
                            {
                                resDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                resDet.Price = resDet.Price + Convert.ToDecimal(resDet.ArticlePairs * artDet.Article.Price);
                                db.Entry(resDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                resDet = new ReserveSaleDetail();
                                resDet.ReserveSaleID = reserve.ReserveSaleID;
                                resDet.ArticleID = artDet.ArticleId;
                                resDet.ArticleDetailID = artDet.ID;
                                resDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                resDet.ArticleCartons = 0;
                                resDet.Price = Convert.ToDecimal(resDet.ArticlePairs * artDet.Article.Price);
                                db.ReserveSaleDetails.Add(resDet);
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

                            var resDet = db.ReserveSaleDetails.FirstOrDefault(c => c.ReserveSaleID == reserve.ReserveSaleID && c.ArticleDetailID == artDet.ID);

                            if (resDet != null)
                            {
                                resDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                resDet.Price = resDet.Price + Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.Entry(resDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                resDet = new ReserveSaleDetail();
                                resDet.ReserveSaleID = reserve.ReserveSaleID;
                                resDet.ArticleID = artDet.ArticleId;
                                resDet.ArticleDetailID = artDet.ID;
                                resDet.ArticlePairs = 0;
                                resDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                resDet.Price = Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.ReserveSaleDetails.Add(resDet);
                                db.SaveChanges();
                            }
                        }
                    }

                    reserve.AmountRecieved = model.AmountPaid;//Convert.ToDecimal(collection["AmountPaid"]);

                    reserve.TotalPrice = db.ReserveSaleDetails.Where(c => c.ReserveSaleID == reserve.ReserveSaleID).Sum(c => c.Price);
                    db.Entry(reserve).State = EntityState.Modified;
                    db.SaveChanges();


                    var custAccount = new CustomerAccountDetail();
                    custAccount.CAccountID = db.Customers.Find(reserve.CustomerID).CustomerAccounts.FirstOrDefault().ID;
                    custAccount.CreatedBy = User.ID;
                    custAccount.CreatedOn = DateTime.UtcNow;
                    custAccount.ReserveID = reserve.ReserveSaleID;
                    custAccount.TotalAmount = model.AmountPaid;
                    custAccount.Description = "RESERVESALE";
                    db.CustomerAccountDetails.Add(custAccount);
                    db.SaveChanges();

                    var CAcount = reserve.Customer.CustomerAccounts.FirstOrDefault();
                    CAcount.TotalPaid = CAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                    CAcount.TotalBalance += reserve.TotalPrice;
                    CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                    CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                    CAcount.UpdatedOn = DateTime.UtcNow;
                    CAcount.UpdatedBy = User.ID;
                    db.Entry(CAcount).State = EntityState.Modified;
                    db.SaveChanges();

                    custAccount.PreviousOutStanding = CAcount.PreviousOutStanding;
                    db.Entry(custAccount).State = EntityState.Modified;
                    db.SaveChanges();
                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseReciept", new { id = reserve.ReserveSaleID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }


            ViewBag.ArticleList = new SelectList(db.ArticleDetails.Where(c => c.TotalStock != 0).Select(c => c.Article).Distinct().ToList(), "ID", "ArticleName");
            ViewBag.CustomerList = new SelectList(db.Customers.ToList(), "ID", "Name");

            List<SelectListItem> CustomerType = new List<SelectListItem>();
            CustomerType.Add(new SelectListItem() { Text = "CREDIT", Value = "CREDIT" });
            CustomerType.Add(new SelectListItem() { Text = "CASH", Value = "CASH" });

            ViewBag.Custype = new SelectList(CustomerType, "Value", "Text");

            return View(model);
        }


        [RoleSecurity(Permissions.ReserveSale, PermissionType.EDIT)]
        public ActionResult Edit(int id)
        {

            var reserve = db.ReserveSales.Find(id);
            if (reserve == null)
                return HttpNotFound();
            EditReserveSaleViewModel model = new EditReserveSaleViewModel();

            model.ResSale = reserve;
            model.ReserveSaleID = reserve.ReserveSaleID;
            model.Article = reserve.ReserveSaleDetails.Select(c => c.Article).Distinct().ToList();

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            ViewBag.CustomerList = new SelectList(db.Customers.ToList(), "ID", "Name", reserve.CustomerID);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EditReserveSaleViewModel model, FormCollection collection)
        {
            var reserve = db.ReserveSales.Find(model.ReserveSaleID);
            try
            { 
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in reserve.ReserveSaleDetails)
                    {
                        ArticleDetail artDet = db.ArticleDetails.Find(item.ArticleDetailID);

                        artDet.Pairs += item.ArticlePairs ?? 0;
                        artDet.Carton += item.ArticleCartons ?? 0;
                        artDet.TotalStock = (artDet.Carton * item.Article.PairInCarton) + artDet.Pairs;

                        db.Entry(artDet).State = EntityState.Modified;
                        db.SaveChanges(); 
                    }
                     
                    var CAcount = reserve.Customer.CustomerAccounts.FirstOrDefault();
                    CAcount.TotalPaid += (-1 * reserve.AmountRecieved);
                    CAcount.TotalBalance += (-1 * reserve.TotalPrice);
                    CAcount.PreviousOutStanding = CAcount.OutStandingAmount;
                    CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
                    CAcount.UpdatedOn = DateTime.UtcNow;
                    CAcount.UpdatedBy = User.ID;

                    db.Entry(CAcount).State = EntityState.Modified;
                    db.SaveChanges();

                    var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.ReserveID == reserve.ReserveSaleID);
                    if (custAccDetail != null)
                    {
                        db.CustomerAccountDetails.Remove(custAccDetail);
                        db.SaveChanges();
                    }


                    db.ReserveSaleDetails.RemoveRange(reserve.ReserveSaleDetails);
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

                            var resDet = db.ReserveSaleDetails.FirstOrDefault(c => c.ReserveSaleID == reserve.ReserveSaleID && c.ArticleDetailID == artDet.ID);
                            if (resDet != null)
                            {
                                resDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                resDet.Price = resDet.Price + Convert.ToDecimal(resDet.ArticlePairs * artDet.Article.Price);
                                db.Entry(resDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                resDet = new ReserveSaleDetail();
                                resDet.ReserveSaleID = reserve.ReserveSaleID;
                                resDet.ArticleID = artDet.ArticleId;
                                resDet.ArticleDetailID = artDet.ID;
                                resDet.ArticlePairs = Convert.ToInt32(collection[item]);
                                resDet.ArticleCartons = 0;
                                resDet.Price = Convert.ToDecimal(resDet.ArticlePairs * artDet.Article.Price);
                                db.ReserveSaleDetails.Add(resDet);
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

                            var resDet = db.ReserveSaleDetails.FirstOrDefault(c => c.ReserveSaleID == reserve.ReserveSaleID && c.ArticleDetailID == artDet.ID);

                            if (resDet != null)
                            {
                                resDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                resDet.Price = resDet.Price + Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.Entry(resDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                resDet = new ReserveSaleDetail();
                                resDet.ReserveSaleID = reserve.ReserveSaleID;
                                resDet.ArticleID = artDet.ArticleId;
                                resDet.ArticleDetailID = artDet.ID;
                                resDet.ArticlePairs = 0;
                                resDet.ArticleCartons = Convert.ToInt32(collection[item]);
                                resDet.Price = Convert.ToDecimal((Convert.ToInt32(collection[item]) * artDet.Article.PairInCarton) * artDet.Article.Price);
                                db.ReserveSaleDetails.Add(resDet);
                                db.SaveChanges();
                            }
                        }
                    }

                    reserve.AmountRecieved = model.AmountPaid; // Convert.ToDecimal(collection["AmountPaid"]);
                    reserve.CustomerID = model.ResSale.CustomerID;
                    reserve.TotalPrice = db.ReserveSaleDetails.Where(c => c.ReserveSaleID == reserve.ReserveSaleID).Sum(c => c.Price);
                    db.Entry(reserve).State = EntityState.Modified;
                    db.SaveChanges();

                    decimal prevout = 0;

                    var custAccount = new CustomerAccountDetail();
                    custAccount.CAccountID = db.Customers.Find(reserve.CustomerID).CustomerAccounts.FirstOrDefault().ID;
                    custAccount.CreatedBy = User.ID;
                    custAccount.CreatedOn = DateTime.UtcNow;
                    custAccount.ReserveID = reserve.ReserveSaleID;
                    custAccount.TotalAmount =   model.AmountPaid;
                    custAccount.Description = "RESERVESALE";
                    db.CustomerAccountDetails.Add(custAccount);
                    db.SaveChanges();

                    var CuAcount = reserve.Customer.CustomerAccounts.FirstOrDefault();
                    CuAcount.TotalPaid = CuAcount.CustomerAccountDetails.Sum(c => c.TotalAmount);
                    CuAcount.TotalBalance += reserve.TotalPrice;
                    prevout = CuAcount.OutStandingAmount;
                    CuAcount.OutStandingAmount = CuAcount.TotalBalance - CuAcount.TotalPaid;
                    CuAcount.UpdatedOn = DateTime.UtcNow;
                    CuAcount.UpdatedBy = User.ID;
                    db.Entry(CuAcount).State = EntityState.Modified;
                    db.SaveChanges();

                    custAccount.PreviousOutStanding = prevout;
                    db.Entry(custAccount).State = EntityState.Modified;
                    db.SaveChanges();
                    trans.Complete();
                    TempData["SUCCESS"] = "ReserveSale added successfully";

                    return RedirectToAction("PurchaseReciept", new { id = reserve.ReserveSaleID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            model.ResSale = reserve;
            model.ReserveSaleID = reserve.ReserveSaleID;
            model.Article = reserve.ReserveSaleDetails.Select(c => c.Article).Distinct().ToList();

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            ViewBag.CustomerList = new SelectList(db.Customers.ToList(), "ID", "Name", reserve.CustomerID);

            return View(model);
        }


        public ActionResult PurchaseReciept(int id)
        {
            var reserve = db.ReserveSales.Find(id);
            if (reserve == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();
            if (reserve.CustomerID != 0)
            {
                Pmodel.CustomerInfo = db.Customers.Find(reserve.CustomerID);
            }

            Pmodel.Date = reserve.AddedOn.ToShortDateString();
            Pmodel.ReserveID = reserve.ReserveSaleID;
            Pmodel.AmountRecieved = reserve.AmountRecieved;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in reserve.ReserveSaleDetails)
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

        [HttpPost]
        public ActionResult GetArticleDetailforReserve(string ArticleID)
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

        public ActionResult GetArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.ArticleDetails.Where(c => c.TotalStock != 0).Select(c => c.Article).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


    }
}