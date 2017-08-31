using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.Orders
{
    public class OrderController : BaseController
    {
        //
        // GET: /Order/
        [RoleSecurity(Permissions.Order, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.Orders.OrderByDescending(c => c.CreatedOn).Select(c => new OrderViewM()
            {
                OrderID = c.OrderID,
                BuyerName = c.Buyer.Name,
                OrderStatus = c.OrderStatus,
                OrderType = c.OrderType,
                TotalPrice = c.TotalPrice

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailRead([DataSourceRequest] DataSourceRequest request, int OrderID)
        {
            var x = db.OrderHistories.Where(c => c.OrderID == OrderID).Select(c => new OrderHisViewM()
            {
                OrderID = c.OrderID,
                Article = db.Articles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.ArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                OrderHisID = c.OrderHistoryID,
                Quantity = c.ArticlePairs,
                RecievedOn = c.RecievedOn
            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
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

        [RoleSecurity(Permissions.Order, PermissionType.ADD)]
        public ActionResult MakeOrder()
        {
            ViewBag.BuyerList = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");

            List<SelectListItem> OrderType = new List<SelectListItem>();
            OrderType.Add(new SelectListItem() { Text = "DIRECT PURCHASE", Value = "1" });
            OrderType.Add(new SelectListItem() { Text = "PLACE NEW ORDER", Value = "2" });

            ViewBag.Ordertype = new SelectList(OrderType, "Value", "Text");

            return View(new PurchaseViewModel());
        }

        [HttpPost]
        public ActionResult MakeOrder(PurchaseViewModel model, FormCollection collection)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    using (var trans = new System.Transactions.TransactionScope())
                    {
                        var order = new Order();

                        order.BuyerID = model.BuyerID;
                        order.CreatedBy = User.ID;
                        order.CreatedOn = DateTime.UtcNow;
                        order.AmountRecieved = 0;
                        order.TotalPrice = 0;
                        if (model.OrderType == 1)
                        {
                            order.OrderType = "DIRECT PURCHASE";
                            order.OrderStatus = "COMPLETE";
                        }
                        else
                        {
                            order.OrderType = "PLACE NEW ORDER";
                            order.OrderStatus = "PENDING";
                        }
                        db.Orders.Add(order);
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
                                if (model.OrderType == 1)
                                {
                                    artDet.TotalStock = (artDet.Carton * artDet.Article.PairInCarton) + Convert.ToInt32(collection[item]);
                                    artDet.Pairs = artDet.TotalStock % artDet.Article.PairInCarton;
                                    artDet.Carton = artDet.TotalStock / artDet.Article.PairInCarton;
                                    db.Entry(artDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                var orderDetail = new OrderDetail();
                                orderDetail.OrderID = order.OrderID;
                                orderDetail.ArticleID = artDet.ArticleId;
                                orderDetail.ArticleDetailID = artDet.ID;
                                orderDetail.ArticlePairs = Convert.ToInt32(collection[item]);
                                orderDetail.Price = (decimal)artDet.Article.Cost * Convert.ToInt32(collection[item]);
                                db.OrderDetails.Add(orderDetail);
                                db.SaveChanges();

                                if (model.OrderType == 1)
                                {
                                    var orderHistory = new OrderHistory();
                                    orderHistory.OrderID = order.OrderID;
                                    orderHistory.OrderDetailID = orderDetail.OrderDetailID;
                                    orderHistory.ArticleID = artDet.ArticleId;
                                    orderHistory.ArticleDetailID = artDet.ID;
                                    orderHistory.ArticlePairs = Convert.ToInt32(collection[item]);
                                    orderHistory.RecievedOn = DateTime.UtcNow;
                                    db.OrderHistories.Add(orderHistory);
                                    db.SaveChanges();
                                }
                            }

                        }

                        order.TotalPrice = (decimal)db.OrderDetails.Where(c => c.OrderID == order.OrderID).Sum(c => c.Price);
                        order.AmountRecieved = model.AmountPaid;
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();

                        var buyerAccont = new BuyerAccountDetail();
                        buyerAccont.BAccountID = db.Buyers.Find(order.BuyerID).BuyerAccounts.FirstOrDefault().ID;
                        buyerAccont.CreatedBy = User.ID;
                        buyerAccont.CreatedOn = DateTime.UtcNow;
                        buyerAccont.OrderID = order.OrderID;

                        if (model.OrderType == 1)
                        {
                            buyerAccont.TotalAmount = order.TotalPrice;
                        }
                        else
                        {
                            buyerAccont.TotalAmount = model.AmountPaid;
                        }

                        db.BuyerAccountDetails.Add(buyerAccont);
                        db.SaveChanges();

                        var BAcount = order.Buyer.BuyerAccounts.FirstOrDefault();
                        BAcount.TotalPaid = BAcount.BuyerAccountDetails.Sum(c => c.TotalAmount);
                        BAcount.TotalBalance += order.TotalPrice;
                        BAcount.OutStandingAmount = BAcount.TotalBalance - BAcount.TotalPaid;
                        BAcount.UpdatedOn = DateTime.UtcNow;
                        BAcount.UpdatedBy = User.ID;
                        db.Entry(BAcount).State = EntityState.Modified;
                        db.SaveChanges();


                        trans.Complete();
                        TempData["SUCCESS"] = "Purchase added successfully";

                        return RedirectToAction("PurchaseReciept", new { id = order.OrderID });
                    }
                }
                catch (Exception ex)
                {
                    TempData["ERROR"] = ex.Message;
                }
            }

            ViewBag.BuyerList = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name");

            List<SelectListItem> OrderType = new List<SelectListItem>();
            OrderType.Add(new SelectListItem() { Text = "DIRECT PURCHASE", Value = "1" });
            OrderType.Add(new SelectListItem() { Text = "PLACE NEW ORDER", Value = "2" });

            ViewBag.Ordertype = new SelectList(OrderType, "Value", "Text");
            return View(model);
        }

        [RoleSecurity(Permissions.Order, PermissionType.EDIT)]
        public ActionResult Edit(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();
            if (order.OrderHistories.Count() > 0)
            {
                TempData["ERROR"] = "Order Cannot be updated";

                return RedirectToAction("Index");
            }

            var model = new EditOrderViewModel();

            model.order = order;
            model.OrderID = order.OrderID;
            model.Article = order.OrderDetails.Select(c => c.Article).Distinct().ToList();
            ViewBag.BuyerList = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name", order.BuyerID);

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EditOrderViewModel model, FormCollection collection)
        {
            var order = db.Orders.Find(model.OrderID);

            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in order.OrderDetails)
                    {
                        ArticleDetail artDet = db.ArticleDetails.Find(item.ArticleDetailID);

                        artDet.Pairs -= item.ArticlePairs ?? 0; 
                        artDet.TotalStock = (artDet.Carton * item.Article.PairInCarton) + artDet.Pairs;

                        db.Entry(artDet).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    var BAcount = order.Buyer.BuyerAccounts.FirstOrDefault();
                    BAcount.TotalPaid += (-1 * order.AmountRecieved);
                    BAcount.TotalBalance += (-1 * order.TotalPrice);
                    BAcount.OutStandingAmount = BAcount.TotalBalance - BAcount.TotalPaid;
                    BAcount.UpdatedOn = DateTime.UtcNow;
                    BAcount.UpdatedBy = User.ID;

                    db.Entry(BAcount).State = EntityState.Modified;
                    db.SaveChanges();

                    var buyAccDetail = db.BuyerAccountDetails.FirstOrDefault(c => c.BAccountID == BAcount.ID && c.OrderID == order.OrderID);
                    if (buyAccDetail != null)
                    {
                        db.BuyerAccountDetails.Remove(buyAccDetail);
                        db.SaveChanges();
                    }
                     
                    db.OrderDetails.RemoveRange(order.OrderDetails);
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

                            artDet.TotalStock += Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;
                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var orderDetail = new OrderDetail();
                            orderDetail.OrderID = order.OrderID;
                            orderDetail.ArticleID = artDet.ArticleId;
                            orderDetail.ArticleDetailID = artDet.ID;
                            orderDetail.ArticlePairs = Convert.ToInt32(collection[item]);
                            orderDetail.Price = (decimal)artDet.Article.Cost * Convert.ToInt32(collection[item]);
                            db.OrderDetails.Add(orderDetail);
                            db.SaveChanges();

                        }
                    }

                    order.BuyerID = model.order.BuyerID;
                    order.TotalPrice = (decimal)db.OrderDetails.Where(c => c.OrderID == order.OrderID).Sum(c => c.Price);
                    order.AmountRecieved = model.AmountPaid;
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();

                    var buyerAccont = new BuyerAccountDetail();
                    buyerAccont.BAccountID = db.Buyers.Find(order.BuyerID).BuyerAccounts.FirstOrDefault().ID;
                    buyerAccont.CreatedBy = User.ID;
                    buyerAccont.CreatedOn = DateTime.UtcNow;
                    buyerAccont.OrderID = order.OrderID; 
                    buyerAccont.TotalAmount = model.AmountPaid;

                    db.BuyerAccountDetails.Add(buyerAccont);
                    db.SaveChanges();

                    var newBuyAccount = order.Buyer.BuyerAccounts.FirstOrDefault();
                    newBuyAccount.TotalPaid = newBuyAccount.BuyerAccountDetails.Sum(c => c.TotalAmount);
                    newBuyAccount.TotalBalance += order.TotalPrice;
                    newBuyAccount.OutStandingAmount = newBuyAccount.TotalBalance - newBuyAccount.TotalPaid;
                    newBuyAccount.UpdatedOn = DateTime.UtcNow;
                    newBuyAccount.UpdatedBy = User.ID;
                    db.Entry(newBuyAccount).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase updated successfully";

                    return RedirectToAction("PurchaseReciept", new { id = order.OrderID });

                }
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }

            model.order = order;
            model.OrderID = order.OrderID;
            model.Article = order.OrderDetails.Select(c => c.Article).Distinct().ToList();
            ViewBag.BuyerList = new SelectList(db.Buyers.Where(c => c.IsActive == true).ToList(), "ID", "Name", order.BuyerID);

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
        }


        [HttpPost]
        public ActionResult GetArticleDetailforPurchase(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.Articles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> <div class='form-group'>";
            result = result + "<input type='hidden' id='item-price-" + artDet.ID + "' value ='" + artDet.Cost + "' /></div>";
            foreach (var item in artDet.ArticleDetails)
            {
                result = result + "<div class='form-group'> <div class='col-sm-3'>" + item.Color.ColorName + " </div>";//<div class='col-sm-3'>Avlailable(" + item.TotalStock + ")</div>";
                result = result + "<div class='col-sm-3'><input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuatePrice(this," + artDet.ID + "," + item.ID + ")' ></input> </div></div>";
                result = result + "<div class='col-sm-3 itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                result = result + "<div class='col-sm-3 itemPrices" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [RoleSecurity(Permissions.Order, PermissionType.VIEW)]
        public ActionResult PurchaseReciept(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();
            Pmodel.AmountRecieved = order.AmountRecieved;
            Pmodel.BuyerInfo = db.Buyers.Find(order.BuyerID);
            Pmodel.PReciept = new List<PurchaseReciept>();
            Pmodel.OrderID = order.OrderID;
            Pmodel.Date = order.CreatedOn.Date.ToShortDateString();
            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();

            foreach (var ITEM in order.OrderDetails)
            {
                if (ITEM.ArticlePairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.ArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = (int)ITEM.ArticlePairs;
                        prD.Price = (decimal)ITEM.Price;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {
                        pr = new PurchaseReciept();
                        pr.Article = ITEM.ArticleID;
                        pr.UnitPrice = ITEM.Article.Cost;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = (int)ITEM.ArticlePairs;
                        prD.Price = (decimal)ITEM.Price;

                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);
                    }
                }
            }

            return View(Pmodel);
        }

        [RoleSecurity(Permissions.Order, PermissionType.ADD)]
        public ActionResult RecieveOrder(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.OrderStatus == "COMPLETE")
            {
                TempData["ERROR"] = "The order status is already completed. Cannot recieve items against it.";

                return RedirectToAction("Index");
            }

            RecieveViewModel Rmodel = new RecieveViewModel();
            Rmodel.OrderID = order.OrderID;

            Rmodel.OrderInfo = new List<OrderArtInfo>();

            OrderArtInfo pr = new OrderArtInfo();
            OrderHis prD = new OrderHis();

            bool OrderHisExist = (order.OrderHistories.Count == 0) ? false : true;

            foreach (var ITEM in order.OrderDetails)
            {
                if (Rmodel.OrderInfo.FindIndex(f => f.ArticleID == ITEM.ArticleID) > 0)
                {
                    prD = new OrderHis();
                    prD.ArticleDetID = ITEM.ArticleDetailID;
                    prD.OrderDetail = ITEM.OrderDetailID;
                    prD.ColorName = ITEM.ArticleDetail.Color.ColorName;
                    prD.TotalOrder = (int)ITEM.ArticlePairs;

                    //  prD.TotalRecieved = (OrderHisExist) ? db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.ArticleDetailID).Sum(c => c.ArticlePairs) : 0;

                    if (OrderHisExist)
                    {
                        var ordrH = db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.ArticleDetailID);
                        if (ordrH.Count() > 0)
                        {
                            prD.TotalRecieved = ordrH.Sum(c => c.ArticlePairs);
                        }
                        else
                        {
                            prD.TotalRecieved = 0;
                        }
                    }
                    else
                    {
                        prD.TotalRecieved = 0;
                    }


                    pr.OrderHis.Add(prD);
                }
                else
                {
                    pr = new OrderArtInfo();
                    pr.ArticleID = ITEM.ArticleID;
                    pr.OrderHis = new List<OrderHis>();
                    prD = new OrderHis();
                    prD.OrderDetail = ITEM.OrderDetailID;
                    prD.ArticleDetID = ITEM.ArticleDetailID;
                    prD.ColorName = ITEM.ArticleDetail.Color.ColorName;
                    prD.TotalOrder = (int)ITEM.ArticlePairs;
                    if (OrderHisExist)
                    {
                        var it = db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.ArticleDetailID);
                        if (it.Count() > 0)
                        {
                            prD.TotalRecieved = it.Sum(c => c.ArticlePairs);
                        }
                        else
                        {
                            prD.TotalRecieved = 0;
                        }
                    }
                    pr.OrderHis.Add(prD);
                    Rmodel.OrderInfo.Add(pr);
                }
            }

            return View(Rmodel);
        }

        [HttpPost]
        public ActionResult RecieveOrder(RecieveViewModel model, FormCollection collec)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in model.OrderInfo)
                    {
                        foreach (var Itd in item.OrderHis)
                        {
                            if (Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]) != 0)
                            {
                                var artd = db.ArticleDetails.Find(Itd.ArticleDetID);

                                artd.TotalStock += Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                artd.Carton = artd.TotalStock / artd.Article.PairInCarton;
                                artd.Pairs = artd.TotalStock % artd.Article.PairInCarton;

                                db.Entry(artd).State = EntityState.Modified;
                                db.SaveChanges();

                                var ordrH = new OrderHistory();
                                ordrH.ArticleID = artd.ArticleId;
                                ordrH.ArticleDetailID = artd.ID;
                                ordrH.ArticlePairs = Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                ordrH.OrderID = model.OrderID;
                                ordrH.OrderDetailID = Itd.OrderDetail;
                                ordrH.RecievedOn = DateTime.UtcNow;

                                db.OrderHistories.Add(ordrH);
                                db.SaveChanges();
                            }
                        }
                    }

                    var order = db.Orders.Find(model.OrderID);
                    var totalItem = order.OrderDetails.Sum(c => c.ArticlePairs);
                    var totalRecieved = order.OrderHistories.Sum(c => c.ArticlePairs);

                    if (totalItem == totalRecieved)
                    {
                        order.OrderStatus = "COMPLETE";
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    trans.Complete();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            return View(model);
        }

        public ActionResult GetArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.Articles.Where(c => c.IsActive == true).ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}