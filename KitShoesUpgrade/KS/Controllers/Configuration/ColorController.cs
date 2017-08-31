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
    public class ColorController : BaseController
    {
        private ColorManager _manager;
        private SecurityPermissions security;
        public ColorController()
        {
            _manager = new ColorManager();
            security = new SecurityPermissions();
        }

        [RoleSecurity(Permissions.Configuration, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(_manager.All().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create([DataSourceRequest] DataSourceRequest request, Color color)
        {
            if (!security.IsAuthorized(User.ROLEID, Permissions.Configuration, PermissionType.ADD))
                ModelState.AddModelError("", "Cannot save Record. You are not authorized to perform.");

            var results = new List<Color>();

            if (color != null && ModelState.IsValid && !ValidationModel.IsEmptyOrWhiteSpace(color.ColorName, ModelState) && ValidationModel.ValidateColor(color, ModelState))
            {
                _manager.Insert(color);
                results.Add(color);
                //   ModelState.AddModelError("", "This record is not approved yet.");
            }

            return Json(results.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update([DataSourceRequest] DataSourceRequest request, Color color)
        {
            if (!security.IsAuthorized(User.ROLEID, Permissions.Configuration, PermissionType.EDIT))
                ModelState.AddModelError("", "Cannot save Record. You are not authorized to perform.");

            if (color != null && ModelState.IsValid && ValidationModel.ValidateColor(color, ModelState))
            {
                var target = _manager.One(p => p.ID == color.ID);
                if (target != null)
                {
                    target.ColorName = color.ColorName;
                    _manager.Update(target);
                }
            }

            return Json(ModelState.ToDataSourceResult(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Destroy([DataSourceRequest] DataSourceRequest request, Color color)
        {
            if (!security.IsAuthorized(User.ROLEID, Permissions.Configuration, PermissionType.DELETE))
                ModelState.AddModelError("", "Cannot save Record. You are not authorized to perform.");

            if (color != null)
            {
                if (db.WHArticleDetails.Any(c => c.ColorId == color.ID))
                    ModelState.AddModelError("", "Cannot delete Record. Child record exists.");
                else
                    _manager.Delete(color);
            }

            return Json(ModelState.ToDataSourceResult(), JsonRequestBehavior.AllowGet);
        }
    }
}
