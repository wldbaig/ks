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
            var a = db.Orders.Where(c => c.SpecialOrder == false).OrderByDescending(c => c.CreatedOn).Select(c => new OrderViewM()
            {
                OrderID = c.OrderID,
                OrderStatus = c.OrderStatus,
                TotalQuantity = c.OrderDetails.Sum(x => x.Pairs)

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

        public ActionResult SP()
        {
            return View();
        }
        public ActionResult ReadSP([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.Orders.Where(c => c.SpecialOrder == true).OrderByDescending(c => c.CreatedOn).Select(c => new OrderViewM()
            {
                OrderID = c.OrderID,
                OrderStatus = c.OrderStatus,
                TotalQuantity = c.SPOrderDetails.Sum(x => x.Total)
            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
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
                        order.SpecialOrder = false;
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


        [RoleSecurity(Permissions.Purchase, PermissionType.ADD)]
        public ActionResult MakeSpecialOrder()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MakeSpecialOrder(FormCollection collection)
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
                        order.SpecialOrder = true;
                        order.OrderStatus = "COMPLETE";

                        db.Orders.Add(order);
                        db.SaveChanges();

                        List<string> keys = new List<string>();

                        for (int i = 1; i < collection.Count; i++)
                        {
                            keys.Add(collection.GetKey(i));
                        }

                        foreach (var item in keys)
                        {
                            if (item.Contains("item-Size6-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size6 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size6 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size7-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size7 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size7 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size8-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size8 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size8 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size9-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size9 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size9 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size10-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size10 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size10 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size11-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size11 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size11 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size12-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size12 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size12 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                            else if (item.Contains("item-Size13-ID*"))
                            {
                                var iDetails = Convert.ToInt32(item.Split('*')[1]);

                                var artDet = db.WHArticleDetails.Find(iDetails);

                                var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                                if (ordDet != null)
                                {
                                    ordDet.Size13 = Convert.ToInt32(collection[item]);
                                    ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                    db.Entry(ordDet).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ordDet = new SPOrderDetail();
                                    ordDet.OrderId = order.OrderID;
                                    ordDet.WHArticleID = artDet.WHArticleId;
                                    ordDet.WHArticleDetailID = artDet.ID;
                                    ordDet.Size13 = Convert.ToInt32(collection[item]);
                                    db.SPOrderDetails.Add(ordDet);
                                    db.SaveChanges();
                                }
                            }
                        }
                        trans.Complete();
                        TempData["SUCCESS"] = "Purchase added successfully";

                        return RedirectToAction("SPReciept", new { id = order.OrderID });

                    }
                }
                catch (Exception ex)
                {
                    TempData["ERROR"] = ex.Message;
                }
            }

            return View();
        }

        public ActionResult EditSpecialOrder(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();


            var model = new EditOrderViewModel();

            model.Order = order;
            model.OrderID = order.OrderID;
            model.Article = order.SPOrderDetails.Select(c => c.WHArticle).Distinct().ToList();

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSpecialOrder(EditOrderViewModel model, FormCollection collection)
        {
            var order = db.Orders.Find(model.OrderID);

            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {

                    db.SPOrderDetails.RemoveRange(order.SPOrderDetails);
                    db.SaveChanges();


                    List<string> keys = new List<string>();

                    for (int i = 1; i < collection.Count; i++)
                    {
                        keys.Add(collection.GetKey(i));
                    }

                    foreach (var item in keys)
                    {
                        if (item.Contains("item-Size6-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size6 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size6 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size7-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size7 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size7 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size8-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size8 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size8 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size9-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size9 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size9 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size10-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size10 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size10 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size11-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size11 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size11 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size12-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size12 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size12 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                        else if (item.Contains("item-Size13-ID*"))
                        {
                            var iDetails = Convert.ToInt32(item.Split('*')[1]);

                            var artDet = db.WHArticleDetails.Find(iDetails);

                            var ordDet = db.SPOrderDetails.FirstOrDefault(c => c.OrderId == order.OrderID && c.WHArticleDetailID == artDet.ID);
                            if (ordDet != null)
                            {
                                ordDet.Size13 = Convert.ToInt32(collection[item]);
                                ordDet.Total = ordDet.Size6 + ordDet.Size7 + ordDet.Size8 + ordDet.Size9 + ordDet.Size10 + ordDet.Size11 + ordDet.Size12 + ordDet.Size13;
                                db.Entry(ordDet).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                ordDet = new SPOrderDetail();
                                ordDet.OrderId = order.OrderID;
                                ordDet.WHArticleID = artDet.WHArticleId;
                                ordDet.WHArticleDetailID = artDet.ID;
                                ordDet.Size13 = Convert.ToInt32(collection[item]);
                                db.SPOrderDetails.Add(ordDet);
                                db.SaveChanges();
                            }
                        }
                    }

                    order.TotalPrice = 0;
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Order updated successfully";

                    return RedirectToAction("SPReciept", new { id = order.OrderID });

                }
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
            }

            model.Order = order;
            model.OrderID = order.OrderID;
            model.Article = order.SPOrderDetails.Select(c => c.WHArticle).Distinct().ToList();

            model.ArtilceList = model.Article.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
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
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.WHArticleID))
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


        public ActionResult SPReciept(int id)
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
            var Groups = order.SPOrderDetails.GroupBy(c => c.WHArticleID, (Id, col) => new { WHArticleID = Id, list = col.ToList() });
            foreach (var ITEM in Groups)
            {
                pr = new PurchaseReciept();
                pr.Article = ITEM.WHArticleID;
                pr.LPRDetail = new List<Models.PRDetail>();
                foreach (var it in ITEM.list)
                {
                    prD = new PRDetail();
                    prD.ArticleDetail = it.WHArticleDetailID;
                    prD.Size6 = it.Size6;
                    prD.Size7 = it.Size7;
                    prD.Size8 = it.Size8;
                    prD.Size9 = it.Size9;
                    prD.Size10 = it.Size10;
                    prD.Size11 = it.Size11;
                    prD.Size12 = it.Size12;
                    prD.Size13 = it.Size13;
                    prD.total = it.Total;


                    pr.LPRDetail.Add(prD);
                 
                }
                Pmodel.PReciept.Add(pr);
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

                    if (OrderHisExist)
                    {
                        var ordrH = db.OrderHistories.Where(c => c.OrderID == ITEM.OrderID && c.OrderDetailID == ITEM.OrderDetailID && c.ArticleDetailID == ITEM.WHArticleDetailID);
                        if (ordrH.Count() > 0)
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

                                    ordrH.TransferTo = "Sheikhupura Shop";
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


        [HttpPost]
        public ActionResult GetSPArticleDetailforPurchase(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.WHArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-2'> <b>" + artDet.ArticleName + " </b></div> <div class='col-sm-1'>Size6</div> <div class='col-sm-1'>Size7</div><div class='col-sm-1'>Size8</div><div class='col-sm-1'>Size9</div><div class='col-sm-1'>Size10</div><div class='col-sm-1'>Size11</div><div class='col-sm-1'>Size12</div><div class='col-sm-1'>Size13</div><div class='col-sm-1'>Total</div></div> ";
            foreach (var item in artDet.WHArticleDetails)
            {
                result = result + "<div class='form-group'> <div class='col-sm-2'>" + item.Color.ColorName + " </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size6-" + item.ID + "' name =  'item-Size6-ID*" + item.ID + "' value = '0' min = '0'  onchange=  'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size7-" + item.ID + "' name =  'item-Size7-ID*" + item.ID + "' value = '0' min = '0'  onchange=  'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size8-" + item.ID + "' name =  'item-Size8-ID*" + item.ID + "' value = '0' min = '0'  onchange=  'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size9-" + item.ID + "' name =  'item-Size9-ID*" + item.ID + "' value = '0' min = '0'  onchange=  'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size10-" + item.ID + "' name = 'item-Size10-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size11-" + item.ID + "' name = 'item-Size11-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size12-" + item.ID + "' name = 'item-Size12-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-Size13-" + item.ID + "' name = 'item-Size13-ID*" + item.ID + "' value = '0' min = '0'  onchange= 'calcuateTotal(" + item.ID + ")' ></input> </div>";
                result = result + "<div class='col-sm-1'><input class='adjust-width numberBox k-widget k-numerictextbox k-input' type='number' id='item-total-" + item.ID + "' name = 'item-total-ID*" + item.ID + "' value = '0' min = '0' readonly ></input> </div>";
                result = result + "</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}