using System;
using System.Linq;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.EPay.Models
{
    public class EWindowModel : BaseNopModel
    {
        public string MerchantNumber { get; set; }

        public string OrderId { get; set; }

        public string Amount { get; set; }

        public string WindowState { get; set; }

        public string Language { get; set; }

        public string Currency { get; set; }

        public string AcceptUrl { get; set; }

        public string CallbackUrl { get; set; }

        public string DeclineUrl { get; set; }

        public string AuthMail { get; set; }
        
        public string Group { get; set; }

        public string InstantCapture { get; set; }

        public string OwnReceipt { get; set; }

        public string Cms { get; set; }

        public string Md5Check { get; set; }
    }
}