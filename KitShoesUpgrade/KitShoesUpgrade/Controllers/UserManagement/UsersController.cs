using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Managers;
using KitShoesUpgrade.Models;
using System;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.UserManagement
{
    [Authorize]
    public class UsersController : BaseController
    {
        //
        // GET: /User/
        private UsersManager _manager;
        public UsersController()
        {
            _manager = new UsersManager();
        }

        [RoleSecurity(Permissions.Users, PermissionType.VIEW)]
        public ActionResult Index()
        {
            return View(_manager.GetUsers());
        }

        [RoleSecurity(Permissions.Users, PermissionType.ADD)]
        public ActionResult Add()
        {
            ViewBag.RoleList = new SelectList(_manager.AllowedRoles(User), "ID", "FullName");
          
            return View(new UserViewModel ());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(UserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_manager.AddUser(model, User.ID))
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, model);
            }

            ViewBag.RoleList = new SelectList(_manager.AllowedRoles(User), "ID", "FullName");
            
            return View(model);
        }


        [RoleSecurity(Permissions.Users, PermissionType.EDIT)]
        public ActionResult Edit(int id = 0)
        {
            UserCredential userCredentials = db.UserCredentials.Find(id);
            if (userCredentials == null)
                return HttpNotFound();

            UserViewModel model = new UserViewModel
            {
                userCredentials = userCredentials
            };

            ViewBag.RoleList = new SelectList(_manager.AllowedRoles(User), "ID", "FullName", model.userCredentials.RoleID);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_manager.EditUser(model, User.ID))
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, model);
            }

            ViewBag.RoleList = new SelectList(_manager.AllowedRoles(User), "ID", "FullName", model.userCredentials.RoleID);

            return View(model);
        }

    }
}