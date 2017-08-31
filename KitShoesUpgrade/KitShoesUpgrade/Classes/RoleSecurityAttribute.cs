using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KitShoesUpgrade.Classes
{
    public class RoleSecurityAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly Permissions permission;
        private readonly CustomPrincipal user;
        private readonly PermissionType permissionType;
        private readonly UserRoles? roles = null;



        public RoleSecurityAttribute(Permissions perm,
            PermissionType permType)
        {
            this.permission = perm;
            this.user = HttpContext.Current.User as CustomPrincipal;
            this.permissionType = permType;
        }
        public RoleSecurityAttribute(Permissions perm,
            PermissionType permType, UserRoles role)
        {
            this.permission = perm;
            this.user = HttpContext.Current.User as CustomPrincipal;
            this.permissionType = permType;
            this.roles = role;
        }

        public void OnAuthorization(AuthorizationContext context)
        {
            var urlHelper = new UrlHelper(context.RequestContext);
            var protocol = context.HttpContext.Request.IsSecureConnection ? "https" : "http";


            if (this.user == null)
            {
                return;
            }

            var sp = new SecurityPermissions();
            if (roles == null)
            {
                if (!sp.IsAuthorized2(user.ROLEID, permission, PermissionType.VIEW))
                {
                    context.Result = new RedirectResult(urlHelper.Action("AccessDenied", "Home"));//, protocol));//MVC.Clients.Actions.Index, MVC.Clients.Name, protocol));
                    return;
                }

                if (!sp.IsAuthorized2(user.ROLEID, permission, permissionType))
                {
                    context.Result = new RedirectResult(urlHelper.Action("AccessDenied", "Home"));//, protocol));//MVC.Clients.Actions.Index, MVC.Clients.Name, protocol));
                    return;
                }
            }
            else
            {
                if (!sp.IsAuthorized2(user.ROLEID, permission, PermissionType.VIEW, roles))
                {
                    context.Result = new RedirectResult(urlHelper.Action("AccessDenied", "Home"));//, protocol));//MVC.Clients.Actions.Index, MVC.Clients.Name, protocol));
                    return;
                }

                if (!sp.IsAuthorized2(user.ROLEID, permission, permissionType, roles))
                {
                    context.Result = new RedirectResult(urlHelper.Action("AccessDenied", "Home"));//, protocol));//MVC.Clients.Actions.Index, MVC.Clients.Name, protocol));
                    return;
                }
            }
            return;
        }
    }
}