using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using static PaymentGateway.Models.PaymentViewModel;
using PaymentGateway.Helper;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    public class PaymentController : Controller
    {

        PaymentHelper _helper = new PaymentHelper();
		
		
        // GET: Payment

        public ActionResult Payment(string aTranId, string CusProfilId = "")
        {
            CardInfoViewModel cardInfoView = new CardInfoViewModel();
            try
            {

                //cardInfoView.CustomerId = "A001";
                //cardInfoView.email = "narsing.m@synsoftglobal.com";
                //cardInfoView.customerProfile_Id = "1960547788";
                //cardInfoView.savecard = GetCardInfo(cardInfoView.customerProfile_Id);
                var requestUrl = $"{ConfigurationManager.AppSettings["GetPaymentInfoUrl"]}";
                var urlParameter = $"?aTranId={aTranId}";
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(requestUrl);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                // List data response.
                HttpResponseMessage response = client.GetAsync(urlParameter).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    var dataObjects = response.Content.ReadAsStringAsync().Result;
                    PaymentInfo obj = JsonConvert.DeserializeObject<PaymentInfo>(dataObjects);
                    cardInfoView.reqId = PaymentInfo.SaveRequest(obj);
                    if (obj.StatusCode == "200")
                    {
                        cardInfoView.email = obj.customeremail;
                        cardInfoView.customerProfile_Id = string.IsNullOrEmpty(obj.CustomerProfileId) ? CusProfilId : obj.CustomerProfileId;//obj.CustomerProfileId == string.Empty? CusProfilId: obj.CustomerProfileId;//"1599538578";
                        cardInfoView.Amount = Math.Round(Convert.ToDecimal(obj.amount), 2);
                        cardInfoView.InvoiceNumber = obj.docType.ToLower() == "order" ? obj.OrderNumber : obj.InvoiceNumber;
                        cardInfoView.ANET_ApiLoginID = obj.ANET_ApiLoginID;
                        cardInfoView.OderNumber = obj.OrderNumber;
                        cardInfoView.ANET_ApiTransactionKey = obj.ANET_ApiTransactionKey;
                        cardInfoView.Environment = obj.env;
                        cardInfoView.aTranId = obj.tranidkey;
                        cardInfoView.taxAmount = obj.taxAmount;
                        cardInfoView.shipping = obj.shipping;
                        cardInfoView.docType = obj.docType;
                        // billing and shiping info
                        cardInfoView.billingFName = obj.billingFName;
                        cardInfoView.billingLName = obj.billingLName;
                        cardInfoView.billingCompany = obj.billingCompany;
                        cardInfoView.billingAddrLine1 = obj.billingAddrLine1;
                        cardInfoView.billingAddressLine2 = obj.billingAddressLine2;
                        cardInfoView.billingCity = obj.billingCity;
                        cardInfoView.billingState = obj.billingState;
                        cardInfoView.billingCountry = obj.billingCountry;
                        cardInfoView.billingZipCode = obj.billingZipCode;
                        cardInfoView.billingPhno = obj.billingPhno;
                        cardInfoView.billingFax = obj.billingFax;
                        cardInfoView.shippingFName = obj.shippingFName;
                        cardInfoView.shippingLName = obj.shippingLName;
                        cardInfoView.shippingCompany = obj.shippingCompany;
                        cardInfoView.shippingAddrLine1 = obj.shippingAddrLine1;
                        cardInfoView.shippingAddressLine2 = obj.shippingAddressLine2;
                        cardInfoView.shippingCity = obj.shippingCity;
                        cardInfoView.shippingState = obj.shippingState;
                        cardInfoView.shippingCountry = obj.shippingCountry;
                        cardInfoView.shippingZipCode = obj.shippingZipCode;
                        cardInfoView.shippingPhno = obj.shippingPhno;
                        cardInfoView.shippingFax = obj.shippingFax;

                        // billing and shiping info
                        if (!string.IsNullOrEmpty(cardInfoView.customerProfile_Id))
                            cardInfoView.savecard = _helper.GetCardInfo(cardInfoView.customerProfile_Id, cardInfoView.ANET_ApiLoginID, cardInfoView.ANET_ApiTransactionKey);
                    }
                    else
                    {
                        return Json($"Error Geting Customer information: {obj.Message}", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    return Json("Error Geting Customer information", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string path = $"{System.Web.HttpContext.Current.Server.MapPath("~/Error")}/{Guid.NewGuid().ToString()}.txt";
                FileSave.FileSaveData(path, ex.Message);
            }

            return View(cardInfoView);
        }
        public ActionResult empty()
        {
            return View();
        }

        public string UpdateCust(CardInfoViewModel data)
        {

            string message = string.Empty;
            try
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = data.ANET_ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = data.ANET_ApiTransactionKey,
                };
                if (data.addPayment.CCID)
                {
                    var creditCard = new creditCardType
                    {
                        cardNumber = data.addnewcard.x_card_num,
                        expirationDate = data.addnewcard.x_exp_code,

                    };
                    // standard api call to retrieve response
                    paymentType cc = new paymentType { Item = creditCard };

                    customerPaymentProfileExType ccPaymentProfile = new customerPaymentProfileExType();
                    ccPaymentProfile.payment = cc;

                    var request = new createCustomerPaymentProfileRequest();
                    request.paymentProfile = ccPaymentProfile;
                    request.customerProfileId = data.customerProfile_Id;

                    // instantiate the controller that will call the service
                    var controller = new createCustomerPaymentProfileController(request);
                    controller.Execute();

                    // get the response from the service (errors contained if any)
                    var response = controller.GetApiResponse();

                    if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        //Console.WriteLine(response.messages.message[0].text);
                    }
                    else if (response != null)
                    {
                        //Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                        //                  response.messages.message[0].text);
                        message = response.messages.message[0].text;
                    }


                }
                else if (data.addPayment.BAccount)
                {
                    var bankAccount = new bankAccountType
                    {
                        accountNumber = data.addPayment.PayAdd_account_num,
                        routingNumber = data.addPayment.ABA_Routing_num,
                        accountType = bankAccountTypeEnum.checking,
                        echeckType = echeckTypeEnum.WEB,
                        nameOnAccount = data.addPayment.PayAdd_name_on_acc,
                        bankName = data.addPayment.PayAdd_bank_name
                    };
                    paymentType echeck = new paymentType { Item = bankAccount };

                    customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();
                    echeckPaymentProfile.payment = echeck;

                    var request = new createCustomerPaymentProfileRequest();
                    request.paymentProfile = echeckPaymentProfile;
                    request.customerProfileId = data.customerProfile_Id;

                    // instantiate the controller that will call the service
                    var controller = new createCustomerPaymentProfileController(request);
                    controller.Execute();

                    // get the response from the service (errors contained if any)
                    var response = controller.GetApiResponse();

                    if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        //Console.WriteLine(response.messages.message[0].text);
                    }
                    else if (response != null)
                    {
                        //Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                        //                  response.messages.message[0].text);
                        message = response.messages.message[0].text;
                    }


                }
                else
                {
                    message = "Some Error ouccerd Please try again!!!";
                }
            }
            catch
            {

            }
            return message;
        }

        [HttpPost]
        public ActionResult PaymentProcess(CardInfoViewModel data)
        {

            if (string.IsNullOrEmpty(data.customerProfile_Id))
                data.customerProfile_Id = _helper.CreateCust(data);

            if (data.addnewcard.isCardSave)
            {
                data.addPayment.CCID = true;

                string message = UpdateCust(data);
                if (string.IsNullOrEmpty(message))
                {
                    var response = new PaymentResponse
                    {
                        aTranId = data.aTranId,
                        cutomerProfileId = data.customerProfile_Id,
                        statusCode = "200",
                        message = "Card Added Successfully!!",
                        transAmount = data.Amount,
                        

                    };
                    _helper.PostPaymentResponse(response, data.reqId);
                    TempData["Success"] = "Card Added Successfully!!";
                }
                else
                {
                    TempData["Error"] = message;
                }
                return RedirectToAction("Payment", "Payment", new { aTranId = data.aTranId });
            }

            if (data.PostType == "Pay")
            {

                if (data.addnewcard.CCAddCard || data.addnewcard.BankAddCard)
                {
                    var response = _helper.PaymentMethod(data);

                    _helper.PostPaymentResponse(response, data.reqId);

                    if (response.statusCode == "200")
                    {
                        return RedirectToAction("ThanKYou", "Payment");
                    }
                    else
                    {
                        TempData["Error"] = response.message;
                    }
                }
                else if (data.addnewcard.SaveCard)
                {

                    var response = _helper.CustTransection(data, data.addnewcard.custumerCardId);
                    response.aTranId = data.aTranId;
                    _helper.PostPaymentResponse(response, data.reqId);
                    if (response.statusCode == "200")
                    {
                        return RedirectToAction("ThanKYou", "Payment");
                    }
                    else
                    {
                        TempData["Error"] = response.message;
                    }
                    return RedirectToAction("Payment", "Payment", new { aTranId = data.aTranId });

                    //foreach (var item in data.savecard)
                    //{
                    //    if (item.CCDynamic)
                    //    {
                    //        var response = _helper.CustTransection(item.customerPaymentProfileId, data.customerProfile_Id, data.Amount, data.ANET_ApiLoginID, data.ANET_ApiTransactionKey);

                    //        if (response.statusCode == "200")
                    //        {
                    //            return RedirectToAction("ThanKYou", "Payment");
                    //        }
                    //        else
                    //        {
                    //            TempData["Error"] = response.message;
                    //        }
                    //    }
                    //}
                }

                if (!data.addnewcard.CCAddCard && !data.addnewcard.BankAddCard)
                {
                    TempData["Error"] = "No Payment Method is Selected";
                }

            }
            else if (data.PostType == "CreateCustomer")
            {
                string cusProfileId = _helper.CreateCust(data);
                if (!string.IsNullOrEmpty(cusProfileId))
                {
                    TempData["Success"] = "Profile Create Successfully!!";
                    return RedirectToAction("Payment", "Payment", new { aTranId = data.aTranId, CusProfilId = cusProfileId });
                }
                else
                {
                    TempData["Error"] = "Error while saving customer!!";
                }
            }
            //else if(data.savecard.)s

            return RedirectToAction("Payment", "Payment", new { aTranId = data.aTranId });
        }

        public ActionResult ThanKYou()
        {
            return View();
        }
    }
}