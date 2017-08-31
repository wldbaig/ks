using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace KS.Classes
{
    public class CommonFunction
    {
        public static string GetBaseUrlForActions(string actionName, string controllerName)
        {

            StringBuilder partialUrl = new StringBuilder();

            partialUrl.Append("/" + controllerName);
            if (actionName != "Index")
            {
                partialUrl.Append("/" + actionName);
            }
            return ConfigurationManager.AppSettings["BaseURL"] + partialUrl.ToString();

        }
    }
}