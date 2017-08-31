using System.Linq;
using System.Web.Mvc;

namespace KS.Models
{
    public class ValidationModel
    {
        static KitSEntities db = new KitSEntities();

        public static bool IsEmptyOrWhiteSpace(string value, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                string errorMessage = "Name cannot be empty or cotain spaces only";
                modelState.AddModelError("", errorMessage);
                return true;
            }
            return false;
        }

        public static bool ValidateArticle(ArticleViewModel model, ModelStateDictionary ModelState)
        {
            if (model.Article.ID == 0)
            {
                if (db.WHArticles.Any(a => a.ArticleName == model.Article.ArticleName))
                {
                    string errorMessage = "Duplicate Name";
                    ModelState.AddModelError("", errorMessage);
                    return false;
                }
            }
            else
            {
                var oldName = db.WHArticles.Find(model.Article.ID).ArticleName;
                if (db.WHArticles.Any(z => z.ArticleName == model.Article.ArticleName && z.ArticleName != oldName))
                {

                    string errorMessage = "Duplicate Name";
                    ModelState.AddModelError("", errorMessage);
                    return false;
                }
            }
            return true;
        }
                                
        public static bool ValidateColor(Color model, ModelStateDictionary ModelState)
        {
            if (model.ID == 0)
            {
                if (db.Colors.Any(a => a.ColorName == model.ColorName ))
                {
                    string errorMessage = "Duplicate Name";
                    ModelState.AddModelError("", errorMessage);
                    return false;
                }
            }
            else
            {
                var oldName = db.Colors.Find(model.ID).ColorName;
                if (db.Colors.Any(z => z.ColorName == model.ColorName && z.ColorName != oldName ))
                {

                    string errorMessage = "Duplicate Name";
                    ModelState.AddModelError("", errorMessage);
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateCategory(Category model, ModelStateDictionary ModelState)
        {
            if (model.ID == 0)
            {
                if (db.Categories.Any(a => a.CategoryName == model.CategoryName))
                {
                    string errorMessage = "Duplicate Name";
                    ModelState.AddModelError("", errorMessage);
                    return false;
                }
            }
            else
            {
                var oldName = db.Categories.Find(model.ID).CategoryName;
                if (db.Categories.Any(z => z.CategoryName == model.CategoryName && z.CategoryName != oldName))
                {

                    string errorMessage = "Duplicate Name";
                    ModelState.AddModelError("", errorMessage);
                    return false;
                }
            }
            return true;
        }
 
    }
}