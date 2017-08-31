using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using KS.Classes;
using KS.Models;
using System.Web.Security;
using Newtonsoft.Json;

namespace KS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                LoggedInUser loginUser = JsonConvert.DeserializeObject<LoggedInUser>(authTicket.UserData);
                CustomPrincipal user = new CustomPrincipal(authTicket.Name);
                user.ID = loginUser.ID;
                user.LOGINID = loginUser.LOGINID;
                user.NAME = loginUser.NAME;

                user.ROLEID = loginUser.ROLEID;
                user.ROLE = loginUser.ROLE;
                user.LASTLOGIN = loginUser.LASTLOGIN;
                user.LOGINCOUNT = loginUser.LOGINCOUNT;
                user.ISACTIVE = loginUser.ISACTIVE;
                user.USERSINCE = loginUser.USERSINCE;

                HttpContext.Current.User = user;
            }
        }
    }
}
