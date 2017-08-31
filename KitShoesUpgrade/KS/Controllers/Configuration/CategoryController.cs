using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using KS.Models;
using KS.Managers;
using KS.Classes;

namespace KS.Controllers.Configuration
{
    public class CategoryController : BaseController
    {
        private CategoryManager _manager;
        private SecurityPermissions security;
        public CategoryController()
        {
            _manager = new CategoryManager();
            security = new SecurityPermissions();
        }

        //
        // GET: /Category/
        [RoleSecurity(Permissions.Configuration, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(_manager.All().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create([DataSourceRequest] DataSourceRequest request, Category category)
        {

            if (!security.IsAuthorized(User.ROLEID, Permissions.Configuration, PermissionType.ADD))
                ModelState.AddModelError("", "Cannot save Record. You are not authorized to perform.");

            var results = new List<Category>();

            if (category != null && ModelState.IsValid && !ValidationModel.IsEmptyOrWhiteSpace(category.CategoryName, ModelState) && ValidationModel.ValidateCategory(category, ModelState))
            {
                _manager.Insert(category);
                results.Add(category);
                //   ModelState.AddModelError("", "This record is not approved yet.");
            }

            return Json(results.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update([DataSourceRequest] DataSourceRequest request, Category category)
        {
            if (!security.IsAuthorized(User.ROLEID, Permissions.Configuration, PermissionType.EDIT))
                ModelState.AddModelError("", "Cannot save Record. You are not authorized to perform.");
            
            if (category != null && ModelState.IsValid && ValidationModel.ValidateCategory(category, ModelState))
            {
                var target = _manager.One(p => p.ID == category.ID);
                if (target != null)
                {
                    target.CategoryName = category.CategoryName;
                    _manager.Update(target);
                }
            }

            return Json(ModelState.ToDataSourceResult(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Destroy([DataSourceRequest] DataSourceRequest request, Category category)
        {
            if (!security.IsAuthorized(User.ROLEID, Permissions.Configuration, PermissionType.DELETE))
                ModelState.AddModelError("", "Cannot save Record. You are not authorized to perform.");
            
            if (category != null)
            {
                if (db.WHArticles.Any(c => c.CategoryId == category.ID))
                    ModelState.AddModelError("", "Cannot delete Record. Child record exists.");
                else
                    _manager.Delete(category);
            }

            return Json(ModelState.ToDataSourceResult(), JsonRequestBehavior.AllowGet);
        }
    }
}