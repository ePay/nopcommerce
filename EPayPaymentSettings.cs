using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.EPay
{
    public class EPayPaymentSettings : ISettings
    {
        public string MerchantId { get; set; }

        public bool FullScreen { get; set; }

        public string Group { get; set; }

        public string Md5Secret { get; set; }

        public string AuthMail { get; set; }

        public string AuthSms { get; set; }

        public bool Instantcapture { get; set; }

        public bool OwnReceipt { get; set; }

        public bool UseRemoteInterface { get; set; }

        public string RemotePassword { get; set; }
    }
}