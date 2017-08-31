using System.Web;
using System.Web.Optimization;

namespace KitShoesUpgrade
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jquerylib").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));


            bundles.Add(new ScriptBundle("~/bundles/mandatoryjs").Include(
                       "~/Scripts/devoops.js" 
                       ));
             

            bundles.Add(new ScriptBundle("~/bundles/kendojs").Include(
                  //  "~/Content/Kendo/js/kendo.all.min.js",
                    "~/Content/Kendo/js/kendo.aspnetmvc.min.js"));


            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js" 
                       
                        )); 

            bundles.Add(new ScriptBundle("~/bundles/cookie").Include("~/Content/js/jquery.cookie.js", "~/Content/js/Site.js"));

            bundles.Add(new StyleBundle("~/bundles/mandatorystyles").Include(
                        "~/plugins/select2/select2.css",
                        "~/Content/css/new-styles.css",
                        "~/Content/font-awesome/css/font-awesome.css" 
                        ));

            bundles.Add(new StyleBundle("~/bundles/admintheme").Include(
                        "~/Content/css/devoops/admin.css"));
             
            bundles.Add(new StyleBundle("~/bundles/css/bootstrap").Include(
                        "~/Content/css/bootstrap.css"
                        )); 
         
        }
    }
}
