using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using KS.Classes;
using KS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KS.Controllers.Return
{
    public class ReturnController : BaseController
    {
        // GET: Return
        [RoleSecurity(Permissions.Return, PermissionType.VIEW)]
        public ActionResult TS()
        {
            return View();
        }

        [RoleSecurity(Permissions.Return, PermissionType.VIEW)]
        public ActionResult BP()
        {
            return View();
        }

        [RoleSecurity(Permissions.Return, PermissionType.VIEW)]
        public ActionResult CM()
        {
            return View();
        }

        [RoleSecurity(Permissions.Return, PermissionType.VIEW)]
        public ActionResult WH()
        {
            return View();
        }
         
        public ActionResult ReadBP([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.BPReturnItems.OrderByDescending(c => c.AddedOn).ToList().Select(c => new ReturnViewM()
            {
                ReturnID = c.ReturnItemID,
                AddedOn = c.AddedOn.Date.ToShortDateString()

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadBP([DataSourceRequest] DataSourceRequest request, int ReturnID)
        {
            var x = db.BPReturnItemDetails.Where(c => c.BPReturnItemID == ReturnID && c.ArticlePairs != 0).Select(c => new ReturnDetViewM()
            {
                ReturnID = c.BPReturnItemID,
                Article = db.BPArticles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                ReturnDetailID = c.ID,
                Quantity = c.ArticlePairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadCM([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.CMReturnItems.OrderByDescending(c => c.AddedOn).ToList().Select(c => new ReturnViewM()
            {
                ReturnID = c.ReturnItemID,
                AddedOn = c.AddedOn.Date.ToShortDateString()

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadCM([DataSourceRequest] DataSourceRequest request, int ReturnID)
        {
            var x = db.CMReturnItemDetails.Where(c => c.CMReturnItemID == ReturnID && c.ArticlePairs != 0).Select(c => new ReturnDetViewM()
            {
                ReturnID = c.CMReturnItemID,
                Article = db.CMArticles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                ReturnDetailID = c.ID,
                Quantity = c.ArticlePairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadWH([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.WHReturnItems.OrderByDescending(c => c.AddedOn).ToList().Select(c => new ReturnViewM()
            {
                ReturnID = c.ReturnItemID,
                AddedOn = c.AddedOn.Date.ToShortDateString()

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadWH([DataSourceRequest] DataSourceRequest request, int ReturnID)
        {
            var x = db.WHReturnItemDetails.Where(c => c.WHReturnItemID == ReturnID && c.ArticlePairs != 0).Select(c => new ReturnDetViewM()
            {
                ReturnID = c.WHReturnItemID,
                Article = db.WHArticles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                ReturnDetailID = c.ID,
                Quantity = c.ArticlePairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult ReadTS([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.CMReturnItems.OrderByDescending(c => c.AddedOn).ToList().Select(c => new ReturnViewM()
            {
                ReturnID = c.ReturnItemID,
                AddedOn = c.AddedOn.Date.ToShortDateString()

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadTS([DataSourceRequest] DataSourceRequest request, int ReturnID)
        {
            var x = db.TSReturnItemDetails.Where(c => c.TSReturnItemID == ReturnID && c.ArticlePairs != 0).Select(c => new ReturnDetViewM()
            {
                ReturnID = c.TSReturnItemID,
                Article = db.TSArticles.FirstOrDefault(o => o.ID == c.ArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.ArticleDetailID).Color.ColorName,
                ReturnDetailID = c.ID,
                Quantity = c.ArticlePairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [RoleSecurity(Permissions.Return, PermissionType.ADD)]
        public ActionResult AddBP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddBP(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var returnitems = new BPReturnItem();

                    returnitems.TotalPrice = 0;
                    returnitems.CreatedBy = User.ID;
                    returnitems.AddedOn = DateTime.UtcNow;

                    db.BPReturnItems.Add(returnitems);
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

                            var artDet = db.BPArticleDetails.Find(iDetails);

                            artDet.TotalStock += Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var retDet = new BPReturnItemDetail();
                            retDet.BPReturnItemID = returnitems.ReturnItemID;
                            retDet.ArticleID = artDet.BPArticleId;
                            retDet.ArticleDetailID = artDet.ID;
                            retDet.ArticlePairs = Convert.ToInt32(collection[item]);
                            retDet.Price = 0;
                            db.BPReturnItemDetails.Add(retDet);
                            db.SaveChanges();


                        }
                      
                    }

                    returnitems.TotalPrice = 0;
                    db.Entry(returnitems).State = EntityState.Modified;
                    db.SaveChanges();
                     
                    trans.Complete();
                    TempData["SUCCESS"] = "Return added successfully";

                    return RedirectToAction("ReturnBPReciept", new { id = returnitems.ReturnItemID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
             

            return View();
        }

        [HttpPost]
        public ActionResult GetArticleDetailforBPReturn(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.BPArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> ";
             
            foreach (var item in artDet.BPArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";
               // result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0'   onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  ></input> </div></div>";
                //result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnBPReciept(int id)
        {
            var ret = db.BPReturnItems.Find(id);
            if (ret == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.ReturnID = ret.ReturnItemID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in ret.BPReturnItemDetails)
            {
                if (ITEM.ArticlePairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.ArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.ArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);

                    }
                }
            }

            return View(Pmodel);
        }

        [RoleSecurity(Permissions.Return, PermissionType.ADD)]
        public ActionResult AddCM()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCM(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var returnitems = new CMReturnItem();

                    returnitems.TotalPrice = 0;
                    returnitems.CreatedBy = User.ID;
                    returnitems.AddedOn = DateTime.UtcNow;

                    db.CMReturnItems.Add(returnitems);
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

                            var artDet = db.CMArticleDetails.Find(iDetails);

                            artDet.TotalStock += Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var retDet = new CMReturnItemDetail();
                            retDet.CMReturnItemID = returnitems.ReturnItemID;
                            retDet.ArticleID = artDet.CMArticleId;
                            retDet.ArticleDetailID = artDet.ID;
                            retDet.ArticlePairs = Convert.ToInt32(collection[item]);
                            retDet.Price = 0;
                            db.CMReturnItemDetails.Add(retDet);
                            db.SaveChanges();


                        }

                    }

                    returnitems.TotalPrice = 0;
                    db.Entry(returnitems).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Return added successfully";

                    return RedirectToAction("ReturnCMReciept", new { id = returnitems.ReturnItemID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }


            return View();
        }

        [HttpPost]
        public ActionResult GetArticleDetailforCMReturn(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.CMArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> ";

            foreach (var item in artDet.CMArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";
                // result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0'   onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  ></input> </div></div>";
                //result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnCMReciept(int id)
        {
            var ret = db.CMReturnItems.Find(id);
            if (ret == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.ReturnID = ret.ReturnItemID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in ret.CMReturnItemDetails)
            {
                if (ITEM.ArticlePairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.ArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.ArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);

                    }
                }
            }

            return View(Pmodel);
        }

        [RoleSecurity(Permissions.Return, PermissionType.ADD)]
        public ActionResult AddTS()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddTS(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var returnitems = new TSReturnItem();

                    returnitems.TotalPrice = 0;
                    returnitems.CreatedBy = User.ID;
                    returnitems.AddedOn = DateTime.UtcNow;

                    db.TSReturnItems.Add(returnitems);
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

                            var artDet = db.TSArticleDetails.Find(iDetails);

                            artDet.TotalStock += Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var retDet = new TSReturnItemDetail();
                            retDet.TSReturnItemID = returnitems.ReturnItemID;
                            retDet.ArticleID = artDet.TSArticleId;
                            retDet.ArticleDetailID = artDet.ID;
                            retDet.ArticlePairs = Convert.ToInt32(collection[item]);
                            retDet.Price = 0;
                            db.TSReturnItemDetails.Add(retDet);
                            db.SaveChanges();


                        }

                    }

                    returnitems.TotalPrice = 0;
                    db.Entry(returnitems).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Return added successfully";

                    return RedirectToAction("ReturnTSReciept", new { id = returnitems.ReturnItemID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }


            return View();
        }

        [HttpPost]
        public ActionResult GetArticleDetailforTSReturn(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.TSArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> ";

            foreach (var item in artDet.TSArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";
                // result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0'   onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  ></input> </div></div>";
                //result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnTSReciept(int id)
        {
            var ret = db.TSReturnItems.Find(id);
            if (ret == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.ReturnID = ret.ReturnItemID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in ret.TSReturnItemDetails)
            {
                if (ITEM.ArticlePairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.ArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.ArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);

                    }
                }
            }

            return View(Pmodel);
        }



        [RoleSecurity(Permissions.Return, PermissionType.ADD)]
        public ActionResult AddWH()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddWH(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var returnitems = new WHReturnItem();

                    returnitems.TotalPrice = 0;
                    returnitems.CreatedBy = User.ID;
                    returnitems.AddedOn = DateTime.UtcNow;

                    db.WHReturnItems.Add(returnitems);
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
                            // For WareHouse return means minus
                            artDet.TotalStock -= Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var retDet = new WHReturnItemDetail();
                            retDet.WHReturnItemID = returnitems.ReturnItemID;
                            retDet.ArticleID = artDet.WHArticleId;
                            retDet.ArticleDetailID = artDet.ID;
                            retDet.ArticlePairs = Convert.ToInt32(collection[item]);
                            retDet.Price = 0;
                            db.WHReturnItemDetails.Add(retDet);
                            db.SaveChanges(); 
                        }

                    }

                    returnitems.TotalPrice = 0;
                    db.Entry(returnitems).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Return added successfully";

                    return RedirectToAction("ReturnWHReciept", new { id = returnitems.ReturnItemID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }


            return View();
        }

        [HttpPost]
        public ActionResult GetArticleDetailforWHReturn(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.WHArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div> ";

            foreach (var item in artDet.WHArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";
                // result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0'   onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0'  ></input> </div></div>";
                //result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnWHReciept(int id)
        {
            var ret = db.WHReturnItems.Find(id);
            if (ret == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.ReturnID = ret.ReturnItemID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in ret.WHReturnItemDetails)
            {
                if (ITEM.ArticlePairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.ArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.ArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.ArticleDetailID;
                        prD.QuantiyAdded = ITEM.ArticlePairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);

                    }
                }
            }

            return View(Pmodel);
        }
         
        public ActionResult GetBPArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.BPArticleDetails.Select(c => c.BPArticle).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCMArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.CMArticleDetails.Select(c => c.CMArticle).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTSArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.TSArticleDetails.Select(c => c.TSArticle).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWHArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.WHArticleDetails.Select(c => c.WHArticle).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}