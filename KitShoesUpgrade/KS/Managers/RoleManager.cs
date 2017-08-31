using KS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace KS.Managers
{
    public class RoleManager : BaseManager
    {
        public bool AddRole(RoleAndPermissions rPermission)
        {
            var rolesIds = "";
            int rpID = 0;


            using (var trans = new TransactionScope())
            {
                // ---- Save/Insert Role
                rPermission.CRole.ChildRoles = (rolesIds == "") ? rolesIds : rolesIds.Remove(rolesIds.Length - 1, 1);
                rPermission.CRole.RoleType = "OTHER";
                rPermission.CRole.CreatedOn = System.DateTime.UtcNow;
                rPermission.CRole.UpdatedOn = System.DateTime.UtcNow;

                db.Roles.Add(rPermission.CRole);
                db.SaveChanges();

                if (rPermission.CRolePermission != null)
                {
                    foreach (var row in rPermission.CRolePermission)
                    {
                        //---- Taking data from CustomRolePermission Viewmodel to Actual RolePermission model to save in db
                        RolePermission rp = new RolePermission();
                        //rpID = db.ROLEPERMISSIONS.Select(r => r.ID).ToArray().Union(Enumerable.Range(0, 1)).Max() + 1;
                        rp.RoleID = rPermission.CRole.ID;// ---- this is the id of the new role inserted
                        rp.PermissionID = row.ID;
                        rp.ID = ++rpID;
                        rp.CanView = (row.canView);
                        rp.CanAdd = (row.canAdd);
                        rp.CanEdit = (row.canEdit);
                        rp.CanDelete = (row.canDelete);

                        db.RolePermissions.Add(rp);
                        db.SaveChanges();
                    }
                }

                trans.Complete();
                return true;
            }
        }

        public bool EditRole(RoleAndPermissions role)
        {
            var roleIds = "";

            using (var trans = new System.Transactions.TransactionScope())
            {
                var rol = db.Roles.Find(role.CRole.ID);
                rol.ChildRoles = (roleIds == "") ? roleIds : roleIds.Remove(roleIds.Length - 1, 1);
                rol.FullName = role.CRole.FullName;
                rol.UpdatedOn = System.DateTime.UtcNow;
                db.Entry(rol).State = EntityState.Modified;
                db.SaveChanges();

                foreach (var row in role.CRolePermission.ToList())
                {
                    var roleEntity = db.RolePermissions.Where(rp => rp.RoleID == role.CRole.ID && rp.PermissionID == row.ID).First();
                    roleEntity.CanView = row.canView;
                    roleEntity.CanAdd = row.canAdd;
                    roleEntity.CanEdit = row.canEdit;
                    roleEntity.CanDelete = row.canDelete;
                    db.SaveChanges();
                }
                trans.Complete();
                return true;
            }
        }
        public List<CustomRolePermission> GellAllPermissions()
        {
            var LCustomPermission = new List<CustomRolePermission>();

            LCustomPermission.AddRange(GetPermissionObject(p => p.ShowDelete == true));
            LCustomPermission.AddRange(GetPermissionObject(p => p.ShowDelete == false && p.ShowAdd == true && p.ShowView == true));
            LCustomPermission.AddRange(GetPermissionObject(p => p.ShowDelete == false && p.ShowView == true && p.ShowAdd == false));

            return LCustomPermission;
        }

        public List<CustomRolePermission> GellAllPermissions(int roleID)
        {
            var LCustomPermission = new List<CustomRolePermission>();

            LCustomPermission.AddRange(GetPermissionObject(p => p.ShowDelete == true, roleID));
            LCustomPermission.AddRange(GetPermissionObject(p => p.ShowDelete == false && p.ShowAdd == true && p.ShowView == true, roleID));
            LCustomPermission.AddRange(GetPermissionObject(p => p.ShowDelete == false && p.ShowAdd == false && p.ShowView == true, roleID));

            return LCustomPermission;
        }

        private List<CustomRolePermission> GetPermissionObject(Func<Permission, bool> p)
        {
            List<CustomRolePermission> LCustomPermission = new List<CustomRolePermission>();

            foreach (var row in db.Permissions.Where(p).OrderBy(s => s.FullName).ToList())
            {
                CustomRolePermission cp = new CustomRolePermission();
                cp.ID = row.ID;
                cp.NAME = row.FullName;
                cp.canAdd = false;
                cp.canDelete = false;
                cp.canEdit = false;
                cp.canView = false;
                cp.showView = row.ShowView;
                cp.showAdd = row.ShowAdd;
                cp.showEdit = row.ShowEdit;
                cp.showDelete = row.ShowDelete;

                LCustomPermission.Add(cp);
            }

            return LCustomPermission;
        }

        private List<CustomRolePermission> GetPermissionObject(Func<Permission, bool> p, int roleID)
        {
            List<CustomRolePermission> LCustomPermission = new List<CustomRolePermission>();

            foreach (var row in db.Permissions.Where(p).OrderBy(s => s.FullName))
            {
                CustomRolePermission cp = new CustomRolePermission();
                cp.ID = row.ID;
                cp.NAME = row.FullName;
                var perm = db.RolePermissions.Where(rp => rp.RoleID == roleID && rp.PermissionID == row.ID).First();
                cp.canAdd = perm.CanAdd;
                cp.canDelete = perm.CanDelete;
                cp.canEdit = perm.CanEdit;
                cp.canView = perm.CanView;

                cp.showView = row.ShowView;
                cp.showAdd = row.ShowAdd;
                cp.showEdit = row.ShowEdit;
                cp.showDelete = row.ShowDelete;

                LCustomPermission.Add(cp);
            }

            return LCustomPermission;
        }
    }
}