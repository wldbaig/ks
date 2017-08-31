using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using KS.Models;
using KS.Managers;
using KS.Classes;
using System.Data.Entity;
using System.Data;


namespace KS.Controllers.Invoices
{
    public class InvoiceController : BaseController
    {
        //
        // GET: /Invoice/
        [RoleSecurity(Permissions.Invoice, PermissionType.VIEW)]
        public ActionResult TS()
        {
            return View();
        }

        [RoleSecurity(Permissions.Invoice, PermissionType.VIEW)]
        public ActionResult BP()
        {
            return View();
        }

        [RoleSecurity(Permissions.Invoice, PermissionType.VIEW)]
        public ActionResult CM()
        {
            return View();
        }

        public ActionResult ReadBP([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.BPInvoices.OrderByDescending(c => c.AddedOn).ToList().Select(c => new InvoiceViewM()
            {
                InvoiceID = c.InvoiceID,
                AddedOn = c.AddedOn.Date.ToShortDateString(),
                TotalQuantity = c.BPInvoiceDetails.Sum(x=>x.Pairs)
                
            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadBP([DataSourceRequest] DataSourceRequest request, int InvoiceID)
        {
            var x = db.BPInvoiceDetails.Where(c => c.BPInvoiceID == InvoiceID && c.Pairs != 0).Select(c => new InvoiceDetViewM()
            {
                InvoiceID = c.BPInvoiceID,
                Article = db.BPArticles.FirstOrDefault(o => o.ID == c.BPArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.BPArticleDetailID).Color.ColorName,
                InvoiceDetailID = c.ID,
                Quantity = c.Pairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadCM([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.CMInvoices.OrderByDescending(c => c.AddedOn).ToList().Select(c => new InvoiceViewM()
            {
                InvoiceID = c.InvoiceID,
                AddedOn = c.AddedOn.Date.ToShortDateString(),
                TotalQuantity = c.CMInvoiceDetails.Sum(x => x.Pairs)

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadCM([DataSourceRequest] DataSourceRequest request, int InvoiceID)
        {
            var x = db.CMInvoiceDetails.Where(c => c.CMInvoiceID == InvoiceID && c.Pairs != 0).Select(c => new InvoiceDetViewM()
            {
                InvoiceID = c.CMInvoiceID,
                Article = db.CMArticles.FirstOrDefault(o => o.ID == c.CMArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.CMArticleDetailID).Color.ColorName,
                InvoiceDetailID = c.ID,
                Quantity = c.Pairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadTS([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.TSInvoices.OrderByDescending(c => c.AddedOn).ToList().Select(c => new InvoiceViewM()
            {
                InvoiceID = c.InvoiceID,
                AddedOn = c.AddedOn.Date.ToShortDateString(),
                TotalQuantity = c.TSInvoiceDetails.Sum(x => x.Pairs)

            }).ToList();

            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailReadTS([DataSourceRequest] DataSourceRequest request, int InvoiceID)
        {
            var x = db.TSInvoiceDetails.Where(c => c.TSInvoiceID == InvoiceID && c.Pairs != 0).Select(c => new InvoiceDetViewM()
            {
                InvoiceID = c.TSInvoiceID,
                Article = db.TSArticles.FirstOrDefault(o => o.ID == c.TSArticleID).ArticleName,
                Color = db.WHArticleDetails.FirstOrDefault(o => o.ID == c.TSArticleDetailID).Color.ColorName,
                InvoiceDetailID = c.ID,
                Quantity = c.Pairs

            }).ToList();

            return Json(x.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [RoleSecurity(Permissions.Invoice, PermissionType.ADD)]
        public ActionResult SaleBP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaleBP(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var invoice = new BPInvoice();

                    invoice.TotalPrice = 0;
                    invoice.CreatedBy = User.ID;
                    invoice.AddedOn = DateTime.UtcNow;

                    db.BPInvoices.Add(invoice);
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

                            artDet.TotalStock += -Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;
                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = new BPInvoiceDetail();
                            invDet.BPInvoiceID = invoice.InvoiceID;
                            invDet.BPArticleID = artDet.BPArticleId;
                            invDet.BPArticleDetailID = artDet.ID;
                            invDet.Pairs = Convert.ToInt32(collection[item]);

                            invDet.Price = artDet.BPArticle.Price ?? 0;
                            db.BPInvoiceDetails.Add(invDet);
                            db.SaveChanges();
                        }
                    }

                    invoice.TotalPrice = db.BPInvoiceDetails.Where(c => c.BPInvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseBPReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            ViewBag.ArticleList = new SelectList(db.BPArticleDetails.Where(c => c.TotalStock != 0).Select(c => c.BPArticle).Distinct().ToList(), "ID", "ArticleName");

            return View();
        }

        [RoleSecurity(Permissions.Invoice, PermissionType.ADD)]
        public ActionResult SaleCM()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaleCM(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var invoice = new CMInvoice();

                    invoice.TotalPrice = 0;
                    invoice.CreatedBy = User.ID;
                    invoice.AddedOn = DateTime.UtcNow;

                    db.CMInvoices.Add(invoice);
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

                            artDet.TotalStock += -Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;
                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = new CMInvoiceDetail();
                            invDet.CMInvoiceID = invoice.InvoiceID;
                            invDet.CMArticleID = artDet.CMArticleId;
                            invDet.CMArticleDetailID = artDet.ID;
                            invDet.Pairs = Convert.ToInt32(collection[item]);

                            invDet.Price = artDet.CMArticle.Price ?? 0;
                            db.CMInvoiceDetails.Add(invDet);
                            db.SaveChanges();
                        }
                    }

                    invoice.TotalPrice = db.CMInvoiceDetails.Where(c => c.CMInvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseCMReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            ViewBag.ArticleList = new SelectList(db.CMArticleDetails.Where(c => c.TotalStock != 0).Select(c => c.CMArticle).Distinct().ToList(), "ID", "ArticleName");

            return View();
        }

        [RoleSecurity(Permissions.Invoice, PermissionType.ADD)]
        public ActionResult SaleTS()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaleTS(FormCollection collection)
        {
            try
            {
                using (var trans = new System.Transactions.TransactionScope())
                {
                    var invoice = new TSInvoice();

                    invoice.TotalPrice = 0;
                    invoice.CreatedBy = User.ID;
                    invoice.AddedOn = DateTime.UtcNow;

                    db.TSInvoices.Add(invoice);
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
                            artDet.DateAdded = DateTime.UtcNow;
                            artDet.TotalStock += -Convert.ToInt32(collection[item]);

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();

                            var invDet = new TSInvoiceDetail();
                            invDet.TSInvoiceID = invoice.InvoiceID;
                            invDet.TSArticleID = artDet.TSArticleId;
                            invDet.TSArticleDetailID = artDet.ID;
                            invDet.Pairs = Convert.ToInt32(collection[item]);

                            invDet.Price = artDet.TSArticle.Price ?? 0;
                            db.TSInvoiceDetails.Add(invDet);
                            db.SaveChanges();
                        }
                    }

                    invoice.TotalPrice = db.TSInvoiceDetails.Where(c => c.TSInvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseTSReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            ViewBag.ArticleList = new SelectList(db.TSArticleDetails.Where(c => c.TotalStock != 0).Select(c => c.TSArticle).Distinct().ToList(), "ID", "ArticleName");

            return View();
        }

        public ActionResult PurchaseBPReciept(int id)
        {
            var invoice = db.BPInvoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.InvoiceID = invoice.InvoiceID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in invoice.BPInvoiceDetails)
            {
                if (ITEM.Pairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.BPArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.BPArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.BPArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.BPArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);

                    }
                }
            }

            return View(Pmodel);
        }

        public ActionResult PurchaseTSReciept(int id)
        {
            var invoice = db.TSInvoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.InvoiceID = invoice.InvoiceID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in invoice.TSInvoiceDetails)
            {
                if (ITEM.Pairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.TSArticleID))
                    {

                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.TSArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.TSArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.TSArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);
                    }
                }
            }

            return View(Pmodel);
        }

        public ActionResult PurchaseCMReciept(int id)
        {
            var invoice = db.CMInvoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            PurchaseRecieptViewModel Pmodel = new PurchaseRecieptViewModel();

            Pmodel.InvoiceID = invoice.InvoiceID;
            Pmodel.PReciept = new List<PurchaseReciept>();

            PurchaseReciept pr = new PurchaseReciept();
            PRDetail prD = new PRDetail();


            foreach (var ITEM in invoice.CMInvoiceDetails)
            {
                if (ITEM.Pairs != 0)
                {
                    if (Pmodel.PReciept.Any(f => f.Article == ITEM.CMArticleID))
                    {
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.CMArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;

                        pr.LPRDetail.Add(prD);
                    }
                    else
                    {

                        pr = new PurchaseReciept();
                        pr.Article = ITEM.CMArticleID;
                        pr.LPRDetail = new List<PRDetail>();
                        prD = new PRDetail();
                        prD.ArticleDetail = ITEM.CMArticleDetailID;
                        prD.QuantiyAdded = ITEM.Pairs;
                        prD.Price = ITEM.Price;
                        pr.LPRDetail.Add(prD);
                        Pmodel.PReciept.Add(pr);
                    }
                }
            }

            return View(Pmodel);
        }

        //public ActionResult CancilInvoice([DataSourceRequest] DataSourceRequest request, InvoiceViewM model)
        //{
        //    try
        //    {
        //        using (var trans = new System.Transactions.TransactionScope())
        //        {
        //            var inv = db.Invoices.Find(model.InvoiceID);

        //            if (inv == null)
        //                return HttpNotFound();

        //            foreach (var item in inv.InvoiceDetails)
        //            {
        //                var artDetId = item.ArticleDetailID;
        //                var carton = item.ArticleCartons;
        //                var pairs = item.ArticlePairs;

        //                var det = db.ArticleDetails.Find(artDetId);

        //                det.Carton += (item.ArticleCartons ?? 0);
        //                det.Pairs += (item.ArticlePairs ?? 0);

        //                det.TotalStock = (det.Carton * det.Article.PairInCarton) + det.Pairs;

        //                db.Entry(det).State = EntityState.Modified;
        //                db.SaveChanges();

        //            }

        //            db.InvoiceDetails.RemoveRange(inv.InvoiceDetails);
        //            db.SaveChanges();

        //            if (inv.CustomerType == "CREDIT")
        //            {
        //                var CAcount = inv.Customer.CustomerAccounts.FirstOrDefault();
        //                CAcount.TotalPaid += (-1 * inv.TotalPrice);
        //                CAcount.TotalBalance += (-1 * inv.TotalPrice);
        //                CAcount.OutStandingAmount = CAcount.TotalBalance - CAcount.TotalPaid;
        //                CAcount.UpdatedOn = DateTime.UtcNow;
        //                CAcount.UpdatedBy = User.ID;
        //                db.Entry(CAcount).State = EntityState.Modified;
        //                db.SaveChanges();

        //                var custAccDetail = db.CustomerAccountDetails.FirstOrDefault(c => c.CAccountID == CAcount.ID && c.InvoiceID == inv.InvoiceID);
        //                if (custAccDetail != null)
        //                {
        //                    db.CustomerAccountDetails.Remove(custAccDetail);
        //                    db.SaveChanges();
        //                }
        //            }

        //            db.Invoices.Remove(inv);
        //            db.SaveChanges();

        //            trans.Complete();

        //            TempData["SUCCESS"] = "Record deleted successfully";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ERROR"] = ex.Message;
        //    }
        //    return RedirectToAction("Index");
        //}

        public ActionResult EditBPInvoice(int id)
        {

            var invoice = db.BPInvoices.Find(id);
            if (invoice == null)
                return HttpNotFound();
            EditInvoiceViewModel model = new EditInvoiceViewModel();

            model.BPInvoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.BPArticle = invoice.BPInvoiceDetails.Select(c => c.BPArticle).Distinct().ToList();

            model.ArtilceList = model.BPArticle.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditBPInvoice(EditInvoiceViewModel model, FormCollection collection)
        {
            var invoice = db.BPInvoices.Find(model.InvoiceID);
            try
            {
                var invId = Convert.ToInt32((collection["InvoiceID"]));
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in invoice.BPInvoiceDetails)
                    {
                        BPArticleDetail artDet = db.BPArticleDetails.Find(item.BPArticleDetailID);

                        artDet.TotalStock += item.Pairs;
                        artDet.DateAdded = DateTime.UtcNow;
                        db.Entry(artDet).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    db.BPInvoiceDetails.RemoveRange(invoice.BPInvoiceDetails);
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

                            artDet.TotalStock += -Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();


                            var invDet = new BPInvoiceDetail();
                            invDet.BPInvoiceID = invoice.InvoiceID;
                            invDet.BPArticleID = artDet.BPArticleId;
                            invDet.BPArticleDetailID = artDet.ID;
                            invDet.Pairs = Convert.ToInt32(collection[item]);
                            db.BPInvoiceDetails.Add(invDet);
                            db.SaveChanges();

                        }

                    }

                    invoice.TotalPrice = db.BPInvoiceDetails.Where(c => c.BPInvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseBPReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            model.BPInvoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.BPArticle = invoice.BPInvoiceDetails.Select(c => c.BPArticle).Distinct().ToList();

            model.ArtilceList = model.BPArticle.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            return View(model);
        }

        public ActionResult EditCMInvoice(int id)
        {

            var invoice = db.CMInvoices.Find(id);
            if (invoice == null)
                return HttpNotFound();
            EditInvoiceViewModel model = new EditInvoiceViewModel();

            model.CMInvoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.CMArticle = invoice.CMInvoiceDetails.Select(c => c.CMArticle).Distinct().ToList();

            model.ArtilceList = model.CMArticle.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditCMInvoice(EditInvoiceViewModel model, FormCollection collection)
        {
            var invoice = db.CMInvoices.Find(model.InvoiceID);
            try
            {
                var invId = Convert.ToInt32((collection["InvoiceID"]));
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in invoice.CMInvoiceDetails)
                    {
                        CMArticleDetail artDet = db.CMArticleDetails.Find(item.CMArticleDetailID);

                        artDet.TotalStock += item.Pairs;
                        artDet.DateAdded = DateTime.UtcNow;

                        db.Entry(artDet).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    db.CMInvoiceDetails.RemoveRange(invoice.CMInvoiceDetails);
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

                            artDet.TotalStock += -Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();


                            var invDet = new CMInvoiceDetail();
                            invDet.CMInvoiceID = invoice.InvoiceID;
                            invDet.CMArticleID = artDet.CMArticleId;
                            invDet.CMArticleDetailID = artDet.ID;
                            invDet.Pairs = Convert.ToInt32(collection[item]);
                            db.CMInvoiceDetails.Add(invDet);
                            db.SaveChanges();

                        }

                    }

                    invoice.TotalPrice = db.CMInvoiceDetails.Where(c => c.CMInvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseCMReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            model.CMInvoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.CMArticle = invoice.CMInvoiceDetails.Select(c => c.CMArticle).Distinct().ToList();

            model.ArtilceList = model.CMArticle.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            return View(model);
        }

        public ActionResult EditTSInvoice(int id)
        {

            var invoice = db.TSInvoices.Find(id);
            if (invoice == null)
                return HttpNotFound();
            EditInvoiceViewModel model = new EditInvoiceViewModel();

            model.TSInvoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.TSArticle = invoice.TSInvoiceDetails.Select(c => c.TSArticle).Distinct().ToList();

            model.ArtilceList = model.TSArticle.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditTSInvoice(EditInvoiceViewModel model, FormCollection collection)
        {
            var invoice = db.TSInvoices.Find(model.InvoiceID);
            try
            {
                var invId = Convert.ToInt32((collection["InvoiceID"]));
                using (var trans = new System.Transactions.TransactionScope())
                {
                    foreach (var item in invoice.TSInvoiceDetails)
                    {
                        TSArticleDetail artDet = db.TSArticleDetails.Find(item.TSArticleDetailID);

                        artDet.TotalStock += item.Pairs;
                        artDet.DateAdded = DateTime.UtcNow;
                        db.Entry(artDet).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    db.TSInvoiceDetails.RemoveRange(invoice.TSInvoiceDetails);
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

                            artDet.TotalStock += -Convert.ToInt32(collection[item]);
                            artDet.DateAdded = DateTime.UtcNow;

                            db.Entry(artDet).State = EntityState.Modified;
                            db.SaveChanges();


                            var invDet = new TSInvoiceDetail();
                            invDet.TSInvoiceID = invoice.InvoiceID;
                            invDet.TSArticleID = artDet.TSArticleId;
                            invDet.TSArticleDetailID = artDet.ID;
                            invDet.Pairs = Convert.ToInt32(collection[item]);
                            db.TSInvoiceDetails.Add(invDet);
                            db.SaveChanges();

                        }

                    }

                    invoice.TotalPrice = db.TSInvoiceDetails.Where(c => c.TSInvoiceID == invoice.InvoiceID).Sum(c => c.Price);
                    db.Entry(invoice).State = EntityState.Modified;
                    db.SaveChanges();

                    trans.Complete();
                    TempData["SUCCESS"] = "Purchase added successfully";

                    return RedirectToAction("PurchaseTSReciept", new { id = invoice.InvoiceID });
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }
            model.TSInvoice = invoice;
            model.InvoiceID = invoice.InvoiceID;
            model.TSArticle = invoice.TSInvoiceDetails.Select(c => c.TSArticle).Distinct().ToList();

            model.ArtilceList = model.TSArticle.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();


            return View(model);
        }


        [HttpPost]
        public ActionResult GetArticleDetailforBPInvoice(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.BPArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div>";
            result = result + " <div class='form-group'><input type='hidden' id='item-price-" + artDet.ID + "' value ='" + artDet.Price + "' /></div>";
            // result = result += "<div class='form-group'> <div class='col-sm-3'  style='width:33.33%'>Color</div> <div class='col-sm-3'  style='width:33.33%'>Carton</div> <div class='col-sm-3'  style='width:33.33%'>Pair</div> </div>";
            foreach (var item in artDet.BPArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";//<div class='col-sm-3'>Avlailable(" + item.TotalStock + ")</div>";
                                                                                                                                             // result = result + "<div  class='col-sm-3' style='width:33.33%'>" + item.Carton + "<input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.Carton + " onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> " + item.TotalStock + " <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.TotalStock + " ></input> </div></div>";
                //   result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //   result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetArticleDetailforCMInvoice(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.CMArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div>";
            result = result + " <div class='form-group'><input type='hidden' id='item-price-" + artDet.ID + "' value ='" + artDet.Price + "' /></div>";
            // result = result += "<div class='form-group'> <div class='col-sm-3'  style='width:33.33%'>Color</div> <div class='col-sm-3'  style='width:33.33%'>Carton</div> <div class='col-sm-3'  style='width:33.33%'>Pair</div> </div>";
            foreach (var item in artDet.CMArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";//<div class='col-sm-3'>Avlailable(" + item.TotalStock + ")</div>";
                                                                                                                                             // result = result + "<div  class='col-sm-3' style='width:33.33%'>" + item.Carton + "<input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.Carton + " onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> " + item.TotalStock + " <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.TotalStock + " ></input> </div></div>";
                //   result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //   result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetArticleDetailforTSInvoice(string ArticleID)
        {
            if (ArticleID == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var artDet = db.TSArticles.FirstOrDefault(c => c.ArticleName == ArticleID);

            var result = "";
            result = result + "<div class='custom' id='" + artDet.ID + "'>  ";
            result = result + " <div class='form-group'> <div class='col-sm-6'> <b>" + artDet.ArticleName + " </b></div> </div>";
            result = result + " <div class='form-group'><input type='hidden' id='item-price-" + artDet.ID + "' value ='" + artDet.Price + "' /></div>";
            // result = result += "<div class='form-group'> <div class='col-sm-3'  style='width:33.33%'>Color</div> <div class='col-sm-3'  style='width:33.33%'>Carton</div> <div class='col-sm-3'  style='width:33.33%'>Pair</div> </div>";
            foreach (var item in artDet.TSArticleDetails)
            {
                result = result + "<div class='form-group'> <div  class='col-sm-2' style='width:33.33%'>" + item.Color.ColorName + " </div>";//<div class='col-sm-3'>Avlailable(" + item.TotalStock + ")</div>";
                                                                                                                                             // result = result + "<div  class='col-sm-3' style='width:33.33%'>" + item.Carton + "<input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-carton-" + item.ID + "' placeholder='Enter Cartons' name = 'item-carton-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.Carton + " onchange= 'calcuatePrice(" + artDet.ID + "," + item.ID + "," + item.Article.PairInCarton + " )' ></input> </div>";
                result = result + "<div  class='col-sm-3' style='width:33.33%'> "+ item.TotalStock + " <input class='numberBox k-widget k-numerictextbox k-input' type='number' id='item-pair-" + item.ID + "'  placeholder='Enter Pairs' name = 'item-color-ID*" + item.ID + "' value = '0' min = '0' max=" + item.TotalStock + " ></input> </div></div>";
                //   result = result + "<div class=' itemPrices' id='artDet-" + item.ID + "' style = 'display:none'>0</div>";
                //   result = result + "<div class=' itemPrices-" + artDet.ID + "' id='artDetr-" + item.ID + "' style = 'display:none'>0</div>";
            }
            result = result + " <hr /></div> ";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBPArticles()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.BPArticleDetails.Where(c => c.TotalStock > 0).Select(c => c.BPArticle).Distinct().ToList();

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
            var result = db.CMArticleDetails.Where(c => c.TotalStock > 0).Select(c => c.CMArticle).Distinct().ToList();

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
            var result = db.TSArticleDetails.Where(c => c.TotalStock > 0).Select(c => c.TSArticle).Distinct().ToList();

            var list = result.Select(c => new ArtilceList()
            {
                ID = c.ID,
                Name = c.ArticleName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}