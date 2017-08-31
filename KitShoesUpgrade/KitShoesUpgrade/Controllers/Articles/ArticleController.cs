using System;
using System.Linq;
using System.Web.Mvc;
using KitShoesUpgrade.Models;
using KitShoesUpgrade.Managers;
using KitShoesUpgrade.Classes;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace KitShoesUpgrade.Controllers.Articles
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
            var a = db.Articles.OrderByDescending(c => c.CreatedOn).Select(c => new ArticleViewM()
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
            var x = db.ArticleDetails.Where(c => c.ArticleId == ArtID).Select(c => new ArtDetV()
            {
                ArtID = c.ArticleId,
                ArtDetID = c.ID,
                Color = c.Color.ColorName,
                Carton = c.Carton,
                Pairs = c.Pairs,
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
            return View(new ArticleViewModel() { Article = new Article() });
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
            var article = db.Articles.Find(id);
            if (article == null)
                return HttpNotFound();

            return View(db.ArticleDetails.Where(s => s.ArticleId == id));
        }

        public ActionResult AddArticleDetails(int id, bool updateMode = true)
        {
            var article = db.Articles.Find(id);
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
                    if (!db.ArticleDetails.Any(c => c.ArticleId == id && c.ColorId == model.ArticleDetails.ColorId))
                    {

                        model.ArticleDetails.ArticleId = id;
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
            var article = db.ArticleDetails.Find(id);
            if (article == null)
                return HttpNotFound();

            ViewBag.ColorList = new SelectList(db.Colors, "ID", "ColorName");
            ViewBag.EditMode = "true";
            return View("ArticleDetailsPopup", new ArticleDetailViewModel() { ArticleDetails = article, ArticleID = article.ArticleId });
        }

        [HttpPost]
        public ActionResult EditArticleDetails(ArticleDetailViewModel model, int id, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _manager.UpdateArticleDetail(model);
                    return RedirectToAction("ArticleDetails", new { id =  model.ArticleDetails.ArticleId });
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

        public ActionResult AddStock(int id)
        {
            var article = db.ArticleDetails.Find(id);
            if (article == null)
                return HttpNotFound();

            return View("ArticleStocksPopup", new ArticleDetailViewModel() { ArticleDetails = article });
        }

        [HttpPost]
        public ActionResult AddStock(ArticleDetailViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _manager.UpdateStock(model);
                    return RedirectToAction("ArticleDetails", new { id = model.ArticleDetails.ArticleId });

                }
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = ex.Message;
            }

            model.ArticleDetails = db.ArticleDetails.Find(model.ArticleDetails.ArticleId);
            return View("ArticleStocksPopup", model);
        }

        [RoleSecurity(Permissions.Articles, PermissionType.EDIT)]
        public ActionResult Edit(int id)
        {
            var article = db.Articles.Find(id);
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

        public ActionResult ViewList()
        {
            return View(db.Categories.ToList());
            //StockModel model = new StockModel();
            //model.Shoes = db.ArticleDetails.Where(c => c.Stock != 0 && c.Article.CategoryId == 1).Select(c => c.Article).Distinct().ToList();
            //model.Material = db.ArticleDetails.Where(c => c.Stock != 0 && c.Article.CategoryId == 2).Select(c => c.Article).Distinct().ToList();
            //model.Raxine = db.ArticleDetails.Where(c => c.Stock != 0 && c.Article.CategoryId == 3).Select(c => c.Article).Distinct().ToList();
            //model.Bakal = db.ArticleDetails.Where(c => c.Stock != 0 && c.Article.CategoryId == 4).Select(c => c.Article).Distinct().ToList();
            //model.Stylo = db.ArticleDetails.Where(c => c.Stock != 0 && c.Article.CategoryId == 5).Select(c => c.Article).Distinct().ToList();

            //model.Upper = db.ArticleDetails.Where(c => c.Stock != 0 && c.Article.CategoryId == 6).Select(c => c.Article).Distinct().ToList();
            //  return View();
        }

        public ActionResult ViewEmptyList()
        {
            return View(db.ArticleDetails.Where(c => c.TotalStock == 0 && c.Article.IsActive == true).Select(c => c.Article).Distinct().ToList());
        }
         
        public ActionResult DeleteStock(int id)
        {
            var artDet = db.ArticleDetails.Find(id);
            if (artDet == null)
                return HttpNotFound();

            var art = artDet.ArticleId;


            if (!db.InvoiceDetails.Any(c => c.ArticleDetailID == id))
            {

                db.ArticleDetails.Remove(artDet);
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
            var article = db.Articles.Find(id);

            if (article == null)
                return HttpNotFound();

            article.IsActive = (article.IsActive) ? false : true;
            db.Entry(article).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}