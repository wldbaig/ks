using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Managers;
using KitShoesUpgrade.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace KitShoesUpgrade.Controllers.UserManagement
{
    public class RolesController : BaseController
    {
        private RoleManager _manager;

        public RolesController()
        {
            _manager = new RoleManager();
        }

        [RoleSecurity(Permissions.Roles, PermissionType.VIEW)]
        public ActionResult Index(Role model = null)
        {
            var roles = db.Roles.OrderBy(r => r.ID).ToList();
            return View(roles);
        }

        [RoleSecurity(Permissions.Roles, PermissionType.ADD)]
        public ActionResult Add()
        {
            RoleAndPermissions RoleAndPermission = new RoleAndPermissions
            {
                CRole = new Role(),
                CRolePermission = _manager.GellAllPermissions()
            };

            ViewBag.ListRoles = new MultiSelectList(db.Roles.OrderBy(r => r.FullName), "ID", "FullName");

            return View(RoleAndPermission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(RoleAndPermissions rPermission)
        {
            try
            {
                if (ModelState.IsValid)
                    if (_manager.AddRole(rPermission))
                        return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                HandleException(ex, rPermission);
            }
            //ViewBag.ListRoles = new MultiSelectList(db.Roles.OrderBy(r => r.FullName), "ID", "FullName");

            return View(rPermission);
        }

        [RoleSecurity(Permissions.Roles, PermissionType.EDIT)]
        public ActionResult Edit(int id = 0)
        {
            Role role = db.Roles.Find(id);
            if (role == null)
                return HttpNotFound();

            var RoleAndPermission = new RoleAndPermissions();

            try
            {
                RoleAndPermission.CRole = role;
                RoleAndPermission.CRolePermission = _manager.GellAllPermissions(role.ID);
            //    RoleAndPermission.RolesList = new List<int>();

                //if (String.IsNullOrEmpty(role.ChildRoles))
                //    ViewBag.ListRoles = new MultiSelectList(db.Roles.Where(r => r.ID != role.ID).OrderBy(r => r.FullName), "ID", "FullName");
                //else
                //{
                //    foreach (var row in role.ChildRoles.Split(',').ToList())
                //    {
                //        RoleAndPermission.RolesList.Add(Convert.ToInt32(row));
                //    }
                //    ViewBag.ListRoles = new MultiSelectList(db.Roles.Where(r => r.ID != role.ID).OrderBy(r => r.FullName), "ID", "FullName", RoleAndPermission.RolesList);
                //}
            }
            catch (Exception ex)
            {
                HandleException(ex, RoleAndPermission);
            }

            return View(RoleAndPermission);
        }

        [HttpPost]
        public ActionResult Edit(RoleAndPermissions model)
        {
            try
            {
                if (db.Roles.Find(model.CRole.ID).RoleType == "ADMIN")
                    return RedirectToAction("Index");

                if (ModelState.IsValid)
                {
                    if (_manager.EditRole(model))
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, model);
            }

            //ViewBag.ListRoles = new MultiSelectList(db.Roles.Where(r => r.ID != model.CRole.ID).OrderBy(r => r.FullName), "ID", "FullName", model.RolesList);

            return View(model);
        }

        //[RoleSecurity(Permissions.Roles, PermissionType.DELETE)]
        //public ActionResult Delete(int id = 0)
        //{
        //    var role = db.Roles.Find(id);
        //    if (role == null)
        //        return HttpNotFound();

        //    try
        //    {
        //        if (db.UserCredentials.Where(c => c.RoleID == id).Count() == 0 && role.RoleType != "ADMIN" && role.RoleType != "OFFICEBOY")
        //        {
        //            using (var trans = new System.Transactions.TransactionScope())
        //            {
        //                db.RolePermissions.RemoveRange(db.RolePermissions.Where(p => p.RoleID == id));
        //                db.Roles.Remove(role);
        //                db.SaveChanges();
        //                trans.Complete();
        //            }
        //        }
        //        else
        //        {
        //            //throw new BAException(eErrorCode.ExistChild);
        //            throw new Exception("You cannot delete a parent record when it's child record exists");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleException(ex);
        //    }
        //    return RedirectToAction("Index");
        //}
    }
}