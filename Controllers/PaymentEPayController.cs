using System;
using System.Text;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Security.Cryptography;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.EPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using System.Collections.Specialized;

namespace Nop.Plugin.Payments.EPay.Controllers
{
    public class PaymentEPayController : BasePaymentController
    {
        private readonly ISettingService settingService;
        private readonly IPaymentService paymentService;
        private readonly IOrderService orderService;
        private readonly IOrderProcessingService orderProcessingService;
        
        private readonly IWebHelper webHelper;
        private readonly EPayPaymentSettings ePayPaymentSettings;
        private readonly PaymentSettings paymentSettings;

        public PaymentEPayController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IWebHelper webHelper,
            EPayPaymentSettings ePayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this.settingService = settingService;
            this.paymentService = paymentService;
            this.orderService = orderService;
            this.orderProcessingService = orderProcessingService;
            this.webHelper = webHelper;
            this.ePayPaymentSettings = ePayPaymentSettings;
            this.paymentSettings = paymentSettings;
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

        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.MerchantId = ePayPaymentSettings.MerchantId;
            model.Group = ePayPaymentSettings.Group;
            model.Md5Secret = ePayPaymentSettings.Md5Secret;
            model.AuthMail = ePayPaymentSettings.AuthMail;
            model.InstantCapture = ePayPaymentSettings.Instantcapture;
            model.OwnReceipt = ePayPaymentSettings.OwnReceipt;
            model.UseRemoteInterface = ePayPaymentSettings.UseRemoteInterface;
            model.RemotePassword = ePayPaymentSettings.RemotePassword;

            return View("Nop.Plugin.Payments.ePay.Views.PaymentePay.Configure", model);
        }

        [HttpPost]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            ePayPaymentSettings.MerchantId = model.MerchantId;
            ePayPaymentSettings.Group = model.Group;
            ePayPaymentSettings.Md5Secret = model.Md5Secret;
            ePayPaymentSettings.AuthMail = model.AuthMail;
            ePayPaymentSettings.Instantcapture = model.InstantCapture;
            ePayPaymentSettings.OwnReceipt = model.OwnReceipt;
            ePayPaymentSettings.UseRemoteInterface = model.UseRemoteInterface;
            ePayPaymentSettings.RemotePassword = model.RemotePassword;
            settingService.SaveSetting(ePayPaymentSettings);
            
            return View("Nop.Plugin.Payments.ePay.Views.PaymentePay.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            return View("Nop.Plugin.Payments.ePay.Views.PaymentePay.PaymentInfo", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult Open(FormCollection form)
        {
            var model = new EWindowModel();

            //Provide properties with values
            model.AcceptUrl = form["accepturl"];
            model.Amount = form["amount"];
            model.AuthMail = form["authmail"];
            model.CallbackUrl = form["callbackurl"];
            model.Cms = form["cms"];
            model.Currency = form["currency"];
            model.DeclineUrl = form["declineurl"];
            model.Group = form["group"];
            model.InstantCapture = form["instantcapture"];
            model.Language = form["language"];
            model.MerchantNumber = form["merchantnumber"];
            model.OrderId = form["orderid"];
            model.OwnReceipt = form["ownreceipt"];
            model.WindowState = form["windowstate"];
			model.Subscription = form["subscription"];
			model.Md5Check = form["md5key"];
            
            return View("Nop.Plugin.Payments.ePay.Views.PaymentePay.EWindow", model);
        }

        [ValidateInput(false)]
        public ActionResult PdtHandler()
        {
            // Get the 3 strings from ePay
            string transactionOrderId = webHelper.QueryString<string>("orderid");
            string transactionTxnId = webHelper.QueryString<string>("txnid");
            string transactionHash = webHelper.QueryString<string>("hash");

            string subscriptionId = webHelper.QueryString<string>("subscriptionid"); // is null if no subscription
			string maskedCCNo = webHelper.QueryString<string>("cardno");

            // Check if ePay module is alive
            var processor = paymentService.LoadPaymentMethodBySystemName("Payments.EPay") as EPayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Error. The ePay module couldn't be loaded!");
                
            // Get order from OrderNumer
            int orderNumber = 0;
            orderNumber = Convert.ToInt32(transactionOrderId);

            // build md5string
            string hash = "";
            NameValueCollection getParameters = HttpContext.Request.QueryString;
            foreach (string key in getParameters)
            {
                if (key != "hash")
                    hash += getParameters[key];
            }
                        
            var order = orderService.GetOrderById(orderNumber);
            
            // Validate payment
            string md5Secret = ePayPaymentSettings.Md5Secret;
            string stringToMd5 = string.Concat(hash, md5Secret);
            string md5Check = GetMD5(stringToMd5);

            bool validated = (md5Check == transactionHash || md5Secret.Length == 0);
            // If order is ok then proceed

            if (order != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("ePay transaction:");
                sb.AppendLine("Transaction ID: " + transactionTxnId);
                sb.AppendLine("Validated transaction : " + validated);

                //order note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = sb.ToString(),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                orderService.UpdateOrder(order);

                if (validated)
                {
					if ( subscriptionId != null )
					{
						order.SubscriptionTransactionId = subscriptionId;
					}

					if (maskedCCNo != null)
					{
						order.MaskedCreditCardNumber = maskedCCNo;
					}

                    if (ePayPaymentSettings.Instantcapture)
                    {
                        if (orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.AuthorizationTransactionId = transactionTxnId;
                            orderProcessingService.MarkOrderAsPaid(order);
                            orderService.UpdateOrder(order);
                        }
                    }
                    else
                    {
                        if (orderProcessingService.CanMarkOrderAsAuthorized(order))
                        {
                            order.AuthorizationTransactionId = transactionTxnId;
                            orderProcessingService.MarkAsAuthorized(order);
                            orderService.UpdateOrder(order);
                        }
                    }
                }
                else
                {
                    throw new NopException("MD5 is not valid.");
                }
            }

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}