using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KS.Models;
using KS.Managers;
using KS.Classes;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace KS.Controllers.Articles
{
    public class ArticleController : BaseController
    {
        ArticleManager _manager;

        public int[] Sizes = new int[] { 550, 300 };
        public ArticleController()
        {
            _manager = new ArticleManager();
        }

        //
        // GET: /Article/
        [RoleSecurity(Permissions.Articles, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }
         
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var a = db.WHArticles.OrderByDescending(c => c.CreatedOn).Select(c => new ArticleViewM()
            {
                ID = c.ID,
                ArticleName = c.ArticleName,
                Category = c.Category.CategoryName,
                Price = c.Price,
                IsActive = c.IsActive,
                Image = c.Image

            }).ToList();
            return Json(a.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailRead([DataSourceRequest] DataSourceRequest request, int ArtID)
        {
            var x = db.WHArticleDetails.Where(c => c.WHArticleId == ArtID).Select(c => new ArtDetV()
            {
                ArtID = c.WHArticleId,
                ArtDetID = c.ID,
                Color = c.Color.ColorName,
                TotalStock = c.TotalStock
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
        
        [RoleSecurity(Permissions.Articles, PermissionType.ADD)]
        public ActionResult Add()
        {
            ViewBag.CategoryList = new SelectList(db.Categories, "ID", "CategoryName");
            return View(new ArticleViewModel() { Article = new WHArticle() });
        }

        [HttpPost]
        public ActionResult Add(ArticleViewModel model)
        {
            try
            {
                if (ModelState.IsValid && ValidationModel.ValidateArticle(model, ModelState))
                {
                    var articleModel = _manager.AddArticle(model, User);

                    return RedirectToAction("ArticleDetails", new { id = articleModel });
                }
            }
            catch (Exception ex)
            {

                TempData["ERROR"] = ex.Message;
            }

            ViewBag.CategoryList = new SelectList(db.Categories, "ID", "CategoryName");
            return View(model);
        }

        public ActionResult ArticleDetails(int id)
        {
            var article = db.WHArticles.Find(id);
            if (article == null)
                return HttpNotFound();

            return View(db.WHArticleDetails.Where(s => s.WHArticleId == id));
        }

        public ActionResult AddArticleDetails(int id, bool updateMode = true)
        {
            var article = db.WHArticles.Find(id);
            if (article == null)
                return HttpNotFound();

            ViewBag.ColorList = new SelectList(db.Colors, "ID", "ColorName");

            return View("ArticleDetailsPopup", new ArticleDetailViewModel() { ArticleID = id });
        }

        [HttpPost]
        public ActionResult AddArticleDetails(ArticleDetailViewModel model, int id, FormCollection form, bool updateMode = true)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!db.WHArticleDetails.Any(c => c.WHArticleId == id && c.ColorId == model.ArticleDetails.ColorId))
                    {

                        model.ArticleDetails.WHArticleId = id;
                        _manager.AddArticleDetail(model);
                    }
                    else
                    {
                        throw new Exception("An article with this color already exist");
                    }
                    if (form["Submit"] == "ItemsList")
                    {
                        return RedirectToAction("ArticleDetails", new { id = id });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            ViewBag.ColorList = new SelectList(db.Colors, "ID", "ColorName");

            return View("ArticleDetailsPopup", model);
        }

        public ActionResult EditArticleDetails(int id)
        {
            var article = db.WHArticleDetails.Find(id);
            if (article == null)
                return HttpNotFound();

            ViewBag.ColorList = new SelectList(db.Colors, "ID", "ColorName");
            ViewBag.EditMode = "true";
            return View("ArticleDetailsPopup", new ArticleDetailViewModel() { ArticleDetails = article, ArticleID = article.WHArticleId });
        }

        [HttpPost]
        public ActionResult EditArticleDetails(ArticleDetailViewModel model, int id, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _manager.UpdateArticleDetail(model);
                    return RedirectToAction("ArticleDetails", new { id =  model.ArticleID });
                }
            }
            catch (Exception ex)
            {

                TempData["ERROR"] = ex.Message;
            }

            ViewBag.ColorList = new SelectList(db.Colors, "ID", "ColorName");
            ViewBag.EditMode = "true";
            return View("ArticleDetailsPopup", model);
        }

       

        [RoleSecurity(Permissions.Articles, PermissionType.EDIT)]
        public ActionResult Edit(int id)
        {
            var article = db.WHArticles.Find(id);
            if (article == null)
                return HttpNotFound();

            ViewBag.CategoryList = new SelectList(db.Categories, "ID", "CategoryName");

            var model = new ArticleViewModel() { Article = article };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ArticleViewModel model, int id)
        {
            if (id != model.Article.ID)
                return HttpNotFound();

            try
            {
                if (ModelState.IsValid && ValidationModel.ValidateArticle(model, ModelState))
                {
                    _manager.UpdateArticle(model, User);
                    return Redirect("~/Article/ArticleDetails/" + id);
                }
            }
            catch (Exception ex)
            {

                TempData["ERROR"] = ex.Message;
            }
            ViewBag.CategoryList = new SelectList(db.Categories, "ID", "CategoryName");

            return View(model);
        }

        public ActionResult ViewWHList()
        {
            return View(db.Categories.ToList()); 
        }

        public ActionResult ViewBPList()
        {
            return View(db.Categories.ToList());
        }

        public ActionResult ViewCMList()
        {
            return View(db.Categories.ToList());
        }

        public ActionResult ViewTSList()
        {
            return View(db.Categories.ToList());
        }

        public ActionResult ViewAllList()
        {
            return View(db.Categories.ToList());
        }

        public ActionResult ViewEmptyList()
        {
            return View(db.WHArticleDetails.Where(c => c.TotalStock == 0 && c.WHArticle.IsActive == true).Select(c => c.WHArticle).Distinct().ToList());
        }
         
        public ActionResult DeleteStock(int id)
        {
            var artDet = db.WHArticleDetails.Find(id);
            var bpArtDet = db.BPArticleDetails.Find(id);
            var cmArtDet = db.CMArticleDetails.Find(id);
            var tsArtDet = db.TSArticleDetails.Find(id);
            if (artDet == null && bpArtDet == null && cmArtDet == null && tsArtDet == null)
                return HttpNotFound();

            var art = artDet.WHArticleId;


            if (!db.BPInvoiceDetails.Any(c => c.BPArticleDetailID == id) || !db.CMInvoiceDetails.Any(c => c.CMArticleDetailID == id) || !db.TSInvoiceDetails.Any(c => c.TSArticleDetailID == id))
            {

                db.WHArticleDetails.Remove(artDet);
                db.BPArticleDetails.Remove(bpArtDet);
                db.TSArticleDetails.Remove(tsArtDet);
                db.CMArticleDetails.Remove(cmArtDet);
                db.SaveChanges();
            }
            else
            {
                TempData["ERROR"] = "Cannot delete this record as it is bind with an invoice";
            }

            return RedirectToAction("ArticleDetails", new { id = art });
        }

        public ActionResult ChangeStatus(int id)
        {
            var article = db.WHArticles.Find(id);
            var bpArticle = db.BPArticles.Find(id);
            var cmArticle = db.CMArticles.Find(id);
            var tsArticle = db.TSArticles.Find(id);

            if (article == null && bpArticle == null && cmArticle == null && tsArticle == null)
                return HttpNotFound();

            article.IsActive = (article.IsActive) ? false : true;
            db.Entry(article).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            bpArticle.IsActive = (bpArticle.IsActive) ? false : true;
            db.Entry(bpArticle).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            cmArticle.IsActive = (cmArticle.IsActive) ? false : true;
            db.Entry(cmArticle).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            tsArticle.IsActive = (tsArticle.IsActive) ? false : true;
            db.Entry(tsArticle).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}