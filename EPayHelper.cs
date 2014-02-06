using System;
using Nop.Core.Domain.Payments;
using System.Globalization;

namespace Nop.Plugin.Payments.EPay
{
    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public class EPayHelper
    {
        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">ePay payment status</param>
        /// <param name="pendingReason">ePay pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "pending":
                    switch (pendingReason.ToLowerInvariant())
                    {
                        case "authorization":
                            result = PaymentStatus.Authorized;
                            break;
                        default:
                            result = PaymentStatus.Pending;
                            break;
                    }
                    break;
                case "processed":
                case "completed":
                case "canceled_reversal":
                    result = PaymentStatus.Paid;
                    break;
                case "denied":
                case "expired":
                case "failed":
                case "voided":
                    result = PaymentStatus.Voided;
                    break;
                case "refunded":
                case "reversed":
                    result = PaymentStatus.Refunded;
                    break;
                default:
                    break;
            }
            return result;
        }
        
        public static int GetLangauge(CultureInfo cultureinfo)
        {
            switch (cultureinfo.ToString())
            {
                case "en-US":
                    return 2;
                case "da-DK":
                    return 1;
                case "sv-SE":
                    return 3;
                default:
                    return 0;
            }
        }
        
        public static string GetIsoCode(string code)
        {
            switch(code.ToUpper())
            {
                case "ADP":
                    return "020";		 
                case "AED":
                    return "784";				    
                case "AFA":
                    return "004";				    
                case "ALL":
                    return "008";				    
                case "AMD":
                    return "051";				    
                case "ANG":
                    return "532";				    
                case "AOA":
                    return "973";				    
                case "ARS":
                    return "032";				    
                case "AUD":
                    return "036";				    
                case "AWG":
                    return "533";				    
                case "AZM":
                    return "031";				    
                case "BAM":
                    return "977";				    
                case "BBD":
                    return "052";				    
                case "BDT":
                    return "050";				    
                case "BGL":
                    return "100";				    
                case "BGN":
                    return "975";				    
                case "BHD":
                    return "048";				    
                case "BIF":
                    return "108";				    
                case "BMD":
                    return "060";			    
                case "BND":
                    return "096";				    
                case "BOB":
                    return "068";				    
                case "BOV":
                    return "984";				    
                case "BRL":
                    return "986";				    
                case "BSD":
                    return "044";				    
                case "BTN":
                    return "064";			    
                case "BWP":
                    return "072";				    
                case "BYR":
                    return "974";				    
                case "BZD":
                    return "084";				    
                case "CAD":
                    return "124";				    
                case "CDF":
                    return "976";				    
                case "CHF":
                    return "756";				    
                case "CLF":
                    return "990";				    
                case "CLP":
                    return "152";				    
                case "CNY":
                    return "156";				    
                case "COP":
                    return "170";				    
                case "CRC":
                    return "188";				    
                case "CUP":
                    return "192";				    
                case "CVE":
                    return "132";				    
                case "CYP":
                    return "196";				    
                case "CZK":
                    return "203";				    
                case "DJF":
                    return "262";			    
                case "DKK":
                    return "208";				    
                case "DOP":
                    return "214";				    
                case "DZD":
                    return "012";				    
                case "ECS":
                    return "218";				    
                case "ECV":
                    return "983";				    
                case "EEK":
                    return "233";				    
                case "EGP":
                    return "818";				    
                case "ERN":
                    return "232";				    
                case "ETB":
                    return "230";				    
                case "EUR":
                    return "978";				    
                case "FJD":
                    return "242";				    
                case "FKP":
                    return "238";				    
                case "GBP":
                    return "826";				    
                case "GEL":
                    return "981";				    
                case "GHC":
                    return "288";				    
                case "GIP":
                    return "292";				    
                case "GMD":
                    return "270";				    
                case "GNF":
                    return "324";				    
                case "GTQ":
                    return "320";				    
                case "GWP":
                    return "624";				    
                case "GYD":
                    return "328";				    
                case "HKD":
                    return "344";			    
                case "HNL":
                    return "340";				    
                case "HRK":
                    return "191";			    
                case "HTG":
                    return "332";				    
                case "HUF":
                    return "348";				    
                case "IDR":
                    return "360";				    
                case "ILS":
                    return "376";		    
                case "INR":
                    return "356";				    
                case "IQD":
                    return "368";				    
                case "IRR":
                    return "364";				    
                case "ISK":
                    return "352";
                case "JMD":
                    return "388";				    
                case "JOD":
                    return "400";				    
                case "JPY":
                    return "392";				    
                case "KES":
                    return "404";				    
                case "KGS":
                    return "417";				    
                case "KHR":
                    return "116";				    
                case "KMF":
                    return "174";				    
                case "KPW":
                    return "408";				    
                case "KRW":
                    return "410";				    
                case "KWD":
                    return "414";				    
                case "KYD":
                    return "136";				    
                case "KZT":
                    return "398";				    
                case "LAK":
                    return "418";				    
                case "LBP":
                    return "422";				    
                case "LKR":
                    return "144";				    
                case "LRD":
                    return "430";				    
                case "LSL":
                    return "426";				    
                case "LTL":
                    return "440";				    
                case "LVL":
                    return "428";				    
                case "LYD":
                    return "434";				    
                case "MAD":
                    return "504";				    
                case "MDL":
                    return "498";				    
                case "MGF":
                    return "450";				    
                case "MKD":
                    return "807";				    
                case "MMK":
                    return "104";				    
                case "MNT":
                    return "496";				    
                case "MOP":
                    return "446";				    
                case "MRO":
                    return "478";				    
                case "MTL":
                    return "470";				    
                case "MUR":
                    return "480";				    
                case "MVR":
                    return "462";				    
                case "MWK":
                    return "454";				    
                case "MXN":
                    return "484";				    
                case "MXV":
                    return "979";				    
                case "MYR":
                    return "458";				    
                case "MZM":
                    return "508";				    
                case "NAD":
                    return "516";				    
                case "NGN":
                    return "566";				    
                case "NIO":
                    return "558";				    
                case "NOK":
                    return "578";				    
                case "NPR":
                    return "524";	    
                case "NZD":
                    return "554";				    
                case "OMR":
                    return "512";				    
                case "PAB":
                    return "590";				    
                case "PEN":
                    return "604";				    
                case "PGK":
                    return "598";				    
                case "PHP":
                    return "608";				    
                case "PKR":
                    return "586";				    
                case "PLN":
                    return "985";				    
                case "PYG":
                    return "600";				    
                case "QAR":
                    return "634";				    
                case "ROL":
                    return "642";				    
                case "RUB":
                    return "643";				    
                case "RUR":
                    return "810";				    
                case "RWF":
                    return "646";				    
                case "SAR":
                    return "682";				    
                case "SBD":
                    return "090";				    
                case "SCR":
                    return "690";				    
                case "SDD":
                    return "736";				    
                case "SEK":
                    return "752";				    
                case "SGD":
                    return "702";				    
                case "SHP":
                    return "654";				    
                case "SIT":
                    return "705";				    
                case "SKK":
                    return "703";				    
                case "SLL":
                    return "694";				    
                case "SOS":
                    return "706";				    
                case "SRG":
                    return "740";				    
                case "STD":
                    return "678";				    
                case "SVC":
                    return "222";				    
                case "SYP":
                    return "760";				    
                case "SZL":
                    return "748";				    
                case "THB":
                    return "764";				    
                case "TJS":
                    return "972";				    
                case "TMM":
                    return "795";				    
                case "TND":
                    return "788";				    
                case "TOP":
                    return "776";				    
                case "TPE":
                    return "626";  
                case "TRL":
                    return "792";
                case "TRY":
                    return "949";
                case "TTD":
                    return "780";
                case "TWD":
                    return "901";
                case "TZS":
                    return "834";
                case "UAH":
                    return "980";
                case "UGX":
                    return "800";
                case "USD":
                    return "840";
                case "UYU":
                    return "858";
                case "UZS":
                    return "860";
                case "VEB":
                    return "862";
                case "VND":
                    return "704";
                case "VUV":
                    return "548";
                case "XAF":
                    return "950";
                case "XCD":
                    return "951";
                case "XOF":
                    return "952";
                case "XPF":
                    return "953";
                case "YER":
                    return "886";
                case "YUM":
                    return "891";
                case "ZAR":
                    return "710";
                case "ZMK":
                    return "894";   
                case "ZWD":
                    return "716";
            }
		
            return "208";
        }
    }
}