using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.EPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.EPay.Configure",
                "Plugins/PaymentePay/Configure",
                new { controller = "PaymentEPay", action = "Configure" },
                new[] { "Nop.Plugin.Payments.EPay.Controllers" });

            routes.MapRoute("Plugin.Payments.EPay.PaymentInfo",
                "Plugins/PaymentePay/PaymentInfo",
                new { controller = "PaymentEPay", action = "PaymentInfo" },
                new[] { "Nop.Plugin.Payments.EPay.Controllers" });

            //PDT
            routes.MapRoute("Plugin.Payments.EPay.PDTHandler",
                "Plugins/PaymentePay/PDTHandler",
                new { controller = "PaymentEPay", action = "PDTHandler" },
                new[] { "Nop.Plugin.Payments.EPay.Controllers" });

            //Open
            routes.MapRoute("Plugin.Payments.EPay.Open",
                "Plugins/PaymentePay/Open",
                new { controller = "PaymentEPay", action = "Open" },
                new[] { "Nop.Plugin.Payments.EPay.Controllers" });
           
            //Cancel
            routes.MapRoute("Plugin.Payments.EPay.CancelOrder",
                "Plugins/PaymentePay/CancelOrder",
                new { controller = "PaymentEPay", action = "CancelOrder" },
                new[] { "Nop.Plugin.Payments.EPay.Controllers" });
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}