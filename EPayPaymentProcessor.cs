using System;
using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.EPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using System.Threading;
using System.Collections.Generic;
using Nop.Web.Framework;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.EPay
{
    /// <summary>
    /// ePay payment processor
    /// </summary>
    public class EPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        
        private readonly EPayPaymentSettings ePayPaymentSettings;
        private readonly ISettingService settingService;
        private readonly IWebHelper webHelper;
		private readonly IOrderService orderService;
        
        #endregion
        
        #region Ctor
        
        public EPayPaymentProcessor(
				EPayPaymentSettings ePayPaymentSettings,
				ISettingService settingService, 
				IWebHelper webHelper,
				IOrderService orderService
		)
        {
            this.ePayPaymentSettings = ePayPaymentSettings;
            this.settingService = settingService;
            this.webHelper = webHelper;
			this.orderService = orderService;
        }
        
        #endregion
        
        #region Utilities
        
        #endregion
        
        #region Methods
        
        public decimal GetAdditionalHandlingFee(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart)
        {
            return decimal.Zero;
        }
        
        public string GetMD5(string inputStr)
        {
            byte[] textBytes = Encoding.Default.GetBytes(inputStr);
            try
            {
                MD5CryptoServiceProvider cryptHandler = new MD5CryptoServiceProvider();
                byte[] hash = cryptHandler.ComputeHash(textBytes);
                string ret = "";
                foreach (byte a in hash)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("x");
                    else
                        ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }
        
        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }
        
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            string lang = EPayHelper.GetLangauge(Thread.CurrentThread.CurrentCulture).ToString();
            
            var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);
            string amount = (orderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
            string currency = EPayHelper.GetIsoCode(postProcessPaymentRequest.Order.CustomerCurrencyCode);
            string itemurl = this.webHelper.GetStoreLocation(false);
            string continueurl = itemurl + "Plugins/PaymentePay/PDTHandler"; 
            string cancelurl = itemurl + "Plugins/PaymentePay/CancelOrder";
            string merchant = ePayPaymentSettings.MerchantId;

			var orderHasRecurringItems = false;
			foreach ( var item in postProcessPaymentRequest.Order.OrderItems )
			{
				orderHasRecurringItems = orderHasRecurringItems || (item.Product != null && item.Product.IsRecurring);
			}

            string ordernumber = postProcessPaymentRequest.Order.Id.ToString("D2");
            // ordernumber 1-9 must be with 0 in front, e.g. 01
            if (ordernumber.Length == 1)
                ordernumber = "0" + ordernumber;
            
            var remotePostHelper = new RemotePost();
            remotePostHelper.FormName = "ePay";
            remotePostHelper.Url = itemurl + "Plugins/PaymentePay/Open";
            
            remotePostHelper.Add("merchantnumber", merchant);
            remotePostHelper.Add("orderid", ordernumber);
            remotePostHelper.Add("amount", amount);
            
            if (ePayPaymentSettings.FullScreen)
                remotePostHelper.Add("windowstate", "3");
            else 
                remotePostHelper.Add("windowstate", "1");
            
            remotePostHelper.Add("language", lang);
            remotePostHelper.Add("currency", currency);
            
            remotePostHelper.Add("accepturl", continueurl);
            remotePostHelper.Add("callbackurl", continueurl);
            remotePostHelper.Add("declineurl", cancelurl);
            
            remotePostHelper.Add("authmail", ePayPaymentSettings.AuthMail);
            remotePostHelper.Add("authsms", ePayPaymentSettings.AuthSms);
            remotePostHelper.Add("group", ePayPaymentSettings.Group);
            
            remotePostHelper.Add("instantcapture", Convert.ToByte(ePayPaymentSettings.Instantcapture).ToString());
            remotePostHelper.Add("ownreceipt", Convert.ToByte(ePayPaymentSettings.OwnReceipt).ToString());
            
            remotePostHelper.Add("cms", "nopcommerce");
            
			remotePostHelper.Add("subscription", (orderHasRecurringItems) ? "1" : "0");

            string stringToMd5 = "";
            foreach (string key in remotePostHelper.Params)
            {
                stringToMd5 += "" + remotePostHelper.Params[key].ToString();
            }
            
            string md5Check = GetMD5(stringToMd5 + ePayPaymentSettings.Md5Secret);
            remotePostHelper.Add("md5key", md5Check);
            
            remotePostHelper.Post();
        }
        
        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee()
        {
            return 0;
        }
        
        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            
            if (ePayPaymentSettings.UseRemoteInterface)
            {
                int pbsResponse = -1;
                int epayresponse = -1;
                
                try
                {
                    var orderTotal = Math.Round(capturePaymentRequest.Order.OrderTotal, 2);
                    string amount = (orderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
                    
                    dk.ditonlinebetalingssystem.ssl.Payment payment = new dk.ditonlinebetalingssystem.ssl.Payment();
                    payment.capture(Convert.ToInt32(ePayPaymentSettings.MerchantId), Convert.ToInt32(capturePaymentRequest.Order.AuthorizationTransactionId), Convert.ToInt32(amount), "", ePayPaymentSettings.RemotePassword, ref pbsResponse, ref epayresponse);
                    
                    if (epayresponse == -1)
                    {
                        result.NewPaymentStatus = PaymentStatus.Paid;
                    }
                    else
                    {
                        result.AddError("Could not capture: pbsResponse:" + pbsResponse.ToString() + " epayresponse: " + epayresponse.ToString());
                    }
                }
                catch (Exception error)
                {
                    result.AddError("Could not capture: " + error.Message);
                }
            }
            else
            {
                result.AddError("Remote interface is not activated.");
            }
            
            return result;
        }
        
        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            
            if (ePayPaymentSettings.UseRemoteInterface)
            {
                int pbsresponse = -1;
                int epayresponse = -1;
                
                try
                {
                    var orderTotal = Math.Round(refundPaymentRequest.AmountToRefund, 2);
                    string amount = (orderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
                    
                    dk.ditonlinebetalingssystem.ssl.Payment payment = new dk.ditonlinebetalingssystem.ssl.Payment();
                    payment.credit(Convert.ToInt32(ePayPaymentSettings.MerchantId), Convert.ToInt32(refundPaymentRequest.Order.AuthorizationTransactionId), Convert.ToInt32(amount), "", ePayPaymentSettings.RemotePassword, ref pbsresponse, ref epayresponse);
                    
                    if (epayresponse == -1)
                    {
                        if (refundPaymentRequest.Order.OrderTotal == refundPaymentRequest.Order.RefundedAmount + refundPaymentRequest.AmountToRefund)
                        {
                            result.NewPaymentStatus = PaymentStatus.Refunded;
                        }
                        else
                        {
                            result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
                        }
                    }
                    else
                    {
                        result.AddError("Could not refund: pbsResponse:" + pbsresponse.ToString() + " epayresponse: " + epayresponse.ToString());
                    }
                }
                catch (Exception error)
                {
                    result.AddError("Could not refund: " + error.Message);
                }
            }
            else
            {
                result.AddError("Remote interface is not activated.");
            }
            
            return result;
        }
        
        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            
            if (ePayPaymentSettings.UseRemoteInterface)
            {
                int epayresponse = -1;
                
                try
                {
                    dk.ditonlinebetalingssystem.ssl.Payment payment = new dk.ditonlinebetalingssystem.ssl.Payment();
                    payment.delete(Convert.ToInt32(ePayPaymentSettings.MerchantId), Convert.ToInt32(voidPaymentRequest.Order.AuthorizationTransactionId), "", ePayPaymentSettings.RemotePassword, ref epayresponse);
                    
                    if (epayresponse == -1)
                    {
                        result.NewPaymentStatus = PaymentStatus.Voided;
                    }
                    else
                    {
                        result.AddError("Could not void: epayresponse:" + epayresponse.ToString());
                    }
                }
                catch (Exception error)
                {
                    result.AddError("Could not void: " + error.Message);
                }
            }
            else
            {
                result.AddError("Remote interface is not activated.");
            }
            
            return result;
        }
        
        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
			if ( !processPaymentRequest.IsRecurringPayment )
			{
				// This payment method doesn't actually handle scheduling recurring payments
				// Therefore the initial call to ProcessRecurringPayment - where it actually
				// ISN'T a recurring payment - is ignored.

				return result;
			}

			var order = orderService.GetOrderById(processPaymentRequest.InitialOrderId);

			if ( order != null ) // this needs an initial order to work (this is where subscriptionid is registered)
			{
				int fraud = 0;
				long transactionid = -1;
				int pbsresponse = -1;
				int epayresponse = -1;

				int merchant = Int32.Parse(ePayPaymentSettings.MerchantId);

				// Construct order identification for this recurring payment
				// This is done because the new order that will hold this payment is not fully constructed yet
				// thus no orderId is available
				var orderId = String.Format("{0}-rec-{1}", processPaymentRequest.InitialOrderId, DateTime.UtcNow.ToString("yyMMddHHmm"));

				// authorize subscription payment
				// instant capture!
				using ( var subscriptionservice = new epay.subscriptionservice.Subscription())
				{
					subscriptionservice.authorize(
							merchant, // merchantid
							Int32.Parse(order.SubscriptionTransactionId), // subscriptionid
							orderId, // orderid
							Convert.ToInt32(Math.Round(processPaymentRequest.OrderTotal, 2) * 100), // amount
							Int32.Parse(EPayHelper.GetIsoCode(order.CustomerCurrencyCode)), // currency
							1, // instantcapture
							"", // group
							"", // description
							"", // email
							"", // sms
							"", // ipaddress
							ePayPaymentSettings.RemotePassword, // pwd
							ref fraud,
							ref transactionid,
							ref pbsresponse,
							ref epayresponse);

					if ( epayresponse == -1 && pbsresponse == 0 )
					{
						result.SubscriptionTransactionId = order.SubscriptionTransactionId;
						result.AuthorizationTransactionId = transactionid.ToString();
						result.CaptureTransactionId = transactionid.ToString();
						result.CaptureTransactionResult = String.Format("Captured: ({0})", transactionid);
						result.NewPaymentStatus = PaymentStatus.Paid;
					}
					else
					{
						var epayresp = -1;
						var pbsresponsetext = "";
						var epayresponsetext = "";

						subscriptionservice.getPbsError(merchant, 2, pbsresponse, ePayPaymentSettings.RemotePassword, ref pbsresponsetext, ref epayresp);
						subscriptionservice.getEpayError(merchant, 2, epayresponse, ePayPaymentSettings.RemotePassword, ref epayresponsetext, ref epayresp);
 
						result.AddError(String.Format("Error processing recurring payment: PBS({0}) EPay({1})", pbsresponsetext, epayresponsetext));
					}
				}
			}
			else
			{
				result.AddError("Initial order wasn't set for recurring payment.");
			}

            return result;
        }
        
        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

			using ( var subscriptionservice = new epay.subscriptionservice.Subscription() )
			{
				var merchant = Int32.Parse(ePayPaymentSettings.MerchantId);

				int epayresponse = -1;
				subscriptionservice.deletesubscription(merchant, Int32.Parse(cancelPaymentRequest.Order.SubscriptionTransactionId), ePayPaymentSettings.RemotePassword, ref epayresponse);

				if ( epayresponse == -1 )
				{
					// ok
					// "empty" result is sufficient
				}
				else
				{
					var epayresp = -1;
					var epayresponsetext = "";

					subscriptionservice.getEpayError(merchant, 2, epayresponse, ePayPaymentSettings.RemotePassword, ref epayresponsetext, ref epayresp);

					result.AddError(String.Format("Cancel recurring failed, code {0} - {1}", epayresponse, epayresponsetext));
				}
			}

            return result;
        }
        
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            
            //ePay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice
            
            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;
            
            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentePay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.EPay.Controllers" }, { "area", null } };
        }
        
        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentEPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.EPay.Controllers" }, { "area", null } };
        }
        
        public Type GetControllerType()
        {
            return typeof(PaymentEPayController);
        }
        
        public override void Install()
        {
            var settings = new EPayPaymentSettings()
            {
                MerchantId = "Oplyses på din ePay konto",
                Group = "",
                Md5Secret = "",
                AuthMail = "",
                AuthSms = "",
                Instantcapture = false,
                OwnReceipt = false,
                UseRemoteInterface = false,
                RemotePassword = ""
            };
            settingService.SaveSetting(settings);
            
            base.Install();
        }
        
        #endregion
        
        #region Properies
        
        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                if (ePayPaymentSettings.UseRemoteInterface)
                    return true;
                else
                    return false;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                if (ePayPaymentSettings.UseRemoteInterface)
                    return true;
                else
                    return false;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                if (ePayPaymentSettings.UseRemoteInterface)
                    return true;
                else
                    return false;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                if (ePayPaymentSettings.UseRemoteInterface)
                    return true;
                else
                    return false;
            }
        }
        
        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
				return RecurringPaymentType.Manual;
            }
        }
        
        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return true;
            }
        }
    
        #endregion
    }
}