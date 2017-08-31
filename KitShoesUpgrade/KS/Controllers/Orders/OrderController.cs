using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using KS.Classes;
using KS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace KS.Controllers.Orders
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
                OrderStatus = c.OrderStatus,
                TotalQuantity = c.OrderDetails.Sum(x=>x.Pairs)

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailRead([DataSourceRequest] DataSourceRequest request, int OrderID)
        {
            var x = db.OrderHistories.Where(c => c.OrderID == OrderID).OrderBy(c => c.TransferTo).ToList().Select(c => new OrderHisViewM()
            {
                OrderID = c.OrderID,
                Article = db.WHArticles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                OrderHisID = c.OrderHistoryID,
                Quantity = c.Pairs,
                TransferDate = c.TransferDate.Date.ToShortDateString(),
                TransferTo = c.TransferTo
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

        [RoleSecurity(Permissions.Purchase, PermissionType.ADD)]
        public ActionResult MakeOrder()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MakeOrder(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var trans = new System.Transactions.TransactionScope())
                    {
                        var order = new Order();

                        order.CreatedBy = User.ID;
                        order.CreatedOn = DateTime.UtcNow;
                        order.TotalPrice = 0;
                        order.OrderStatus = "PENDING";

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

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                artDet.TotalStock += Convert.ToInt32(collection[item]);
                                artDet.DateAdded = DateTime.UtcNow;
                                db.Entry(artDet).State = EntityState.Modified;
                                db.SaveChanges();

                                var orderDetail = new OrderDetail();
                                orderDetail.OrderID = order.OrderID;
                                orderDetail.WHArticleID = artDet.WHArticleId;
                                orderDetail.WHArticleDetailID = artDet.ID;
                                orderDetail.Pairs = Convert.ToInt32(collection[item]);
                                orderDetail.Price = Convert.ToInt32(collection[item]);
                                db.OrderDetails.Add(orderDetail);
                                db.SaveChanges();

                            }
                        }

                        order.TotalPrice = (decimal)db.OrderDetails.Where(c => c.OrderID == order.OrderID).Sum(c => c.Price);
                        db.Entry(order).State = EntityState.Modified;
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

            return View();
        }

        [RoleSecurity(Permissions.Purchase, PermissionType.EDIT)]
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

            model.Order = order;
            model.OrderID = order.OrderID;
            model.Article = order.OrderDetails.Select(c => c.WHArticle).Distinct().ToList();

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
                        WHArticleDetail artDet = db.WHArticleDetails.Find(item.WHArticleDetailID);
                        artDet.TotalStock -= item.Pairs;
                        artDet.DateAdded = DateTime.UtcNow;
                        db.Entry(artDet).State = EntityState.Modified;
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

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            artDet.TotalStock += Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;
                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var orderDetail = new OrderDetail();
                            orderDetail.OrderID = order.OrderID;
                            orderDetail.WHArticleID = artDet.WHArticleId;
                            orderDetail.WHArticleDetailID = artDet.ID;
                            orderDetail.Pairs = Convert.ToInt32(collection[item]);
                            orderDetail.Price = Convert.ToInt32(collection[item]);
                            db.OrderDetails.Add(orderDetail);
                            db.SaveChanges();

                        }
                    }

                    order.TotalPrice = (decimal)db.OrderDetails.Where(c => c.OrderID == order.OrderID).Sum(c => c.Price);
                    db.Entry(order).State = EntityState.Modified;
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
             
            model.Order = order;
            model.OrderID = order.OrderID;
            model.Article = order.OrderDetails.Select(c => c.WHArticle).Distinct().ToList();

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
            var artDet = db.WHArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> ";
            //      result = result + "<div class='form-group'><input type='hidden' id='item-price-" + artDet.ID + "' value ='" + artDet.Price + "' /></div>";
            foreach (var item in artDet.WHArticleDetails)
            {
                result = result + "<div class='form-group'> <div class='col-sm-3'>" + item.Color.ColorName + " </div>";//<div class='col-sm-3'>Avlailable(" + item.TotalStock + ")</div>";
                //result = result + "<div class='col-sm-3'><input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuatePrice(this," + artDet.ID + "," + item.ID + ")' ></input> </div></div>";
                result = result + "<div class='col-sm-3'><input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuatePrice(this," + artDet.ID + "," + item.ID + ")' ></input> </div></div>";
                //result = result + "<div class='col-sm-3 itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //result = result + "<div class='col-sm-3 itemPrices" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }
         
        public ActionResult PurchaseReciept(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();
            Pmodel.OrderID = order.OrderID;

            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();

            foreach (var ITEM in order.OrderDetails)
            {
                if (ITEM.Pairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.WHArticleID) )
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.WHArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;
                         
                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {
                        pr = new PurchaseReciept();
                        pr.Article = ITEM.WHArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.WHArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;
                         
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);
                    }
                }
            }

            return View(Pmodel);
        }

        [RoleSecurity(Permissions.Purchase, PermissionType.ADD)]
        public ActionResult RecieveOrder(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.OrderStatus == "COMPLETE")
            {
                TempData["ERROR"] = "The order status is already completed. Cannot transfer items against it.";

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
                if (Rmodel.OrderInfo.FindIndex(f => f.ArticleID == ITEM.WHArticleID) > 0)
                {
                    prD = new OrderHis();
                    prD.ArticleDetID = ITEM.WHArticleDetailID;
                    prD.OrderDetail = ITEM.OrderDetailID;
                    prD.ColorName = ITEM.WHArticleDetail.Color.ColorName;
                    prD.TotalOrder = ITEM.Pairs;

                   // prD.TotalRecieved = (OrderHisExist) ? db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.WHArticleDetailID).Sum(c => c.Pairs) : 0;

                    if(OrderHisExist)
                    {
                        var ordrH = db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.WHArticleDetailID);
                        if(ordrH.Count() > 0)
                        {
                            prD.TotalRecieved = ordrH.Sum(c => c.Pairs);
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
                    pr.ArticleID = ITEM.WHArticleID;
                    pr.OrderHis = new List<OrderHis>();
                    prD = new OrderHis();
                    prD.OrderDetail = ITEM.OrderDetailID;
                    prD.ArticleDetID = ITEM.WHArticleDetailID;
                    prD.ColorName = ITEM.WHArticleDetail.Color.ColorName;
                    prD.TotalOrder = ITEM.Pairs;
                    if (OrderHisExist)
                    {
                        var it = db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.WHArticleDetailID);
                        if (it.Count() > 0)
                        {
                            prD.TotalRecieved = it.Sum(c => c.Pairs);
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

            ViewBag.Shops = new SelectList(db.Shops.Where(c => c.ID != 1).ToList(), "ID", "ShopName");

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
                                var artd = db.WHArticleDetails.Find(Itd.ArticleDetID);

                                artd.TotalStock -= Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                artd.DateAdded = DateTime.UtcNow;

                                db.Entry(artd).State = EntityState.Modified;
                                db.SaveChanges();

                                var ordrH = new OrderHistory();
                                ordrH.ArticleID = artd.WHArticleId;
                                ordrH.ArticleDetailID = artd.ID;
                                ordrH.Pairs = Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                ordrH.OrderID = model.OrderID;
                                ordrH.OrderDetailID = Itd.OrderDetail;
                                ordrH.TransferDate = DateTime.UtcNow;

                                if (model.TransferTo == 2)
                                {
                                    var bpArtd = db.BPArticleDetails.Find(Itd.ArticleDetID);

                                    bpArtd.TotalStock += Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                    bpArtd.DateAdded = DateTime.UtcNow;
                                    db.Entry(bpArtd).State = EntityState.Modified;
                                    db.SaveChanges();

                                    ordrH.TransferTo = "BhagwanPura";
                                }

                                if (model.TransferTo == 3)
                                {
                                    var cmArtd = db.CMArticleDetails.Find(Itd.ArticleDetID);

                                    cmArtd.TotalStock += Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                    cmArtd.DateAdded = DateTime.UtcNow;
                                    db.Entry(cmArtd).State = EntityState.Modified;
                                    db.SaveChanges();

                                    ordrH.TransferTo = "ChahMiran";
                                }

                                if (model.TransferTo == 4)
                                {
                                    var tsArtd = db.TSArticleDetails.Find(Itd.ArticleDetID);

                                    tsArtd.TotalStock += Convert.ToInt32(collec["recieved-" + Itd.ArticleDetID]);
                                    tsArtd.DateAdded = DateTime.UtcNow;
                                    db.Entry(tsArtd).State = EntityState.Modified;
                                    db.SaveChanges();

                                    ordrH.TransferTo = "ThirdShop";
                                }

                                db.OrderHistories.Add(ordrH);
                                db.SaveChanges();
                            }
                        }
                    }

                    var order = db.Orders.Find(model.OrderID);
                    var totalItem = order.OrderDetails.Sum(c => c.Pairs);
                    var totalRecieved = order.OrderHistories.Sum(c => c.Pairs);

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
            var result = db.WHArticles.ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}