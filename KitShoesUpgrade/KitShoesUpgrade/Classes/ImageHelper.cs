using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Classes
{
    public enum eDirectory
    {
        Article = 1
    }

    public static class ImageHelper
    {           
        public static IHtmlString Source(eDirectory dir, string size, object image )
        {
            var baseUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ImageURL"];
            string source = baseUrl + "/images/";
            switch (dir)
            {
                case eDirectory.Article:
                    var article = (Article)image;
                    source += (int)eDirectory.Article + "/" + article.ID % 10 + "/image_" + size + "_" + article.ID + Path.GetExtension(article.Image);
                    break;

            }
            return new HtmlString(source);
        }
    }

}