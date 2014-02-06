using System.ComponentModel;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.EPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [DisplayName("Merchantnumber")]
        public string MerchantId { get; set; }

        [DisplayName("Use full screen")]
        public bool FullScreen { get; set; }

        [DisplayName("Group")]
        public string Group { get; set; }

        [DisplayName("MD5 Key")]
        public string Md5Secret { get; set; }

        [DisplayName("Auth Mail")]
        public string AuthMail { get; set; }

        [DisplayName("Instant capture")]
        public bool InstantCapture { get; set; }

        [DisplayName("Own receipt")]
        public bool OwnReceipt { get; set; }

        [DisplayName("Use Remote Interface")]
        public bool UseRemoteInterface { get; set; }

        [DisplayName("Remote Password")]
        public string RemotePassword { get; set; }
    }
}