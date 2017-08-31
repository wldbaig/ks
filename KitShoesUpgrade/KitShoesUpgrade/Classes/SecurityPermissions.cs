using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Classes
{
    public class SecurityPermissions
    {
        KSEntities db = new KSEntities();



        public bool IsAuthorized(int? roleID, Permissions permID, PermissionType type)
        {

            var user = HttpContext.Current.User as CustomPrincipal;
            roleID = (int)user.ROLEID;

            var result = db.RolePermissions.FirstOrDefault(rp => rp.RoleID == roleID && rp.PermissionID == (int)permID);

            switch (type)
            {
                case PermissionType.ADD:
                    if (result.CanAdd)
                        return true;
                    break;
                case PermissionType.EDIT:
                    if (result.CanEdit)
                        return true;
                    break;
                case PermissionType.DELETE:
                    if (result.CanDelete)
                        return true;
                    break;
                case PermissionType.VIEW:
                    if (result.CanView)
                        return true;
                    break;
            }

            return false;
        }

        public bool IsAuthorized2(int? roleID, Permissions permID, PermissionType type)
        {
            return IsAuthorized2(roleID, permID, type, null);
        }

        public bool IsAuthorized2(int? roleID, Permissions permID, PermissionType type, UserRoles? roles)
        {
            var user = HttpContext.Current.User as CustomPrincipal;
            roleID = (int)user.ROLEID;
            switch (roles)
            {
                case UserRoles.Administrators:
                    if (roleID != db.RolePermissions.FirstOrDefault(c => c.Role.RoleType == "ADMIN").RoleID)
                        return false;
                    break;
                case UserRoles.Operator:
                    if (roleID != db.RolePermissions.FirstOrDefault(c => c.Role.RoleType == "OPERATOR").RoleID)
                        return false;
                    break;
                case UserRoles.Other:
                    if (roleID != db.RolePermissions.FirstOrDefault(c => c.Role.RoleType == "OTHER").RoleID)
                        return false;
                    break;
            }

            var result = db.RolePermissions.FirstOrDefault(rp => rp.RoleID == roleID && rp.PermissionID == (int)permID);

            switch (type)
            {
                case PermissionType.ADD:
                    if (result.CanAdd)
                        return true;
                    break;
                case PermissionType.EDIT:
                    if (result.CanEdit)
                        return true;
                    break;
                case PermissionType.DELETE:
                    if (result.CanDelete)
                        return true;
                    break;
                case PermissionType.VIEW:
                    if (result.CanView)
                        return true;
                    break;
            }

            return false;
        }
    }

    public enum PermissionType { VIEW, ADD, EDIT, DELETE };

    public enum Permissions
    {
        Users = 1,
        Roles,
        Configuration,
        Articles,
        Invoice,
        ReserveSale,
        CashReciept,
        Customer,
        CustomerAccount,
        Buyer,
        BuyerAccount ,
        Order,
        Report,
        Return 
    }

    public enum UserRoles
    {
        Administrators = 1,
        Operator,
        Other
    }

    public enum eErrorCode
    {
        InvalidLoginID = 1,
        InvalidCredentials,
        DuplicateLoginID,
        IncorrectPassword,
        NoUserFound,

    }

}