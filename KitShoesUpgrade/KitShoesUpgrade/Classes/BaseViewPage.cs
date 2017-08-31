using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KitShoesUpgrade.Classes
{
    public abstract class BaseViewPage : WebViewPage
    {
        public virtual new CustomPrincipal User
        {
            get { return base.User as CustomPrincipal; }
        }

        public virtual string BaseURL
        {
            get { return System.Web.Configuration.WebConfigurationManager.AppSettings["BaseURL"]; }
        }
    }
    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new CustomPrincipal User
        {
            get { return base.User as CustomPrincipal; }
        }

        public virtual string BaseURL
        {
            get { return System.Web.Configuration.WebConfigurationManager.AppSettings["BaseURL"]; }
        }

    }
}