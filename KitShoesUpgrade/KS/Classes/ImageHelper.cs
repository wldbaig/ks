using System.IO;
using System.Web;

namespace KS.Classes
{
    public enum eDirectory
    {
        WHArticle = 1
    }

    public static class ImageHelper
    {
        public static IHtmlString Source(eDirectory dir, string size, int ID, string image)
        {
            var baseUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ImageURL"];
            string source = baseUrl + "/images/";
            switch (dir)
            {
                case eDirectory.WHArticle: 
                    source += (int)eDirectory.WHArticle + "/" +  ID % 10 + "/image_" + size + "_" + ID + Path.GetExtension(image);
                    break;

            }
            return new HtmlString(source);
        }
    }

}