using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AKV.ExamEnq
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("dumpfile.ashx");

            routes.MapRoute(
                name: "BankReturnUrl",
                url: "bank/returnurl",
                defaults: new { controller = "Bank", action = "ReturnUrl" }
            );

            routes.MapRoute(
                name: "BankStartPay",
                url: "bank/startpay",
                defaults: new { controller = "Bank", action = "StartPay" }
            );

            routes.MapRoute(
                name: "BankAffApp",
                url: "bank/AffApp",
                defaults: new { controller = "AffApp", action = "Index" }
            );

            routes.MapRoute(
                name: "BankAffAppActions",
                url: "bank/AffApp/{action}/{id}",
                defaults: new { controller = "AffApp", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
