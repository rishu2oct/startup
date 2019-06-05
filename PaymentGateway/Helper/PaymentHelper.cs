using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using DbProviderFactorie;
using Newtonsoft.Json;
using PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using static PaymentGateway.Models.PaymentViewModel;

namespace PaymentGateway.Helper
{
    /// <summary>
    /// All the payment related code will come here 
    /// </summary>
    public class PaymentHelper
    {
        public List<SaveCardViewModel> GetCardInfo(string CustomerProfileId, string ANET_ApiLoginID, string ANET_ApiTransactionKey)
        {
            //SaveCardViewModel saveCardView = new SaveCardViewModel();
            List<SaveCardViewModel> lstsaveCards = new List<SaveCardViewModel>();
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ANET_ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ANET_ApiTransactionKey,

            };

            var request = new getCustomerProfileRequest();
            request.customerProfileId = CustomerProfileId;

            // instantiate the controller that will call the service
            var controller = new getCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                Console.WriteLine(response.messages.message[0].text);
                Console.WriteLine("Customer Profile Id: " + response.profile.customerProfileId);

                if (response.subscriptionIds != null && response.subscriptionIds.Length > 0)
                {
                    Console.WriteLine("List of subscriptions : ");
                    for (int i = 0; i < response.subscriptionIds.Length; i++)
                        Console.WriteLine(response.subscriptionIds[i]);
                }
                for (int i = 0; i < response.profile.paymentProfiles.Length; i++)
                {
                    SaveCardViewModel saveCardView = new SaveCardViewModel();
                    if (response.profile.paymentProfiles[i].payment.Item.GetType() == typeof(creditCardMaskedType))
                    {
                        creditCardMaskedType objItem = (creditCardMaskedType)response.profile.paymentProfiles[i].payment.Item;
                        saveCardView.cardNumber = objItem.cardNumber;
                        saveCardView.cardType = objItem.cardType;
                        saveCardView.expirationDate = objItem.expirationDate;
                        saveCardView.customerPaymentProfileId = response.profile.paymentProfiles[i].customerPaymentProfileId;
                    }
                    else if (response.profile.paymentProfiles[i].payment.Item.GetType() == typeof(bankAccountMaskedType))
                    {
                        bankAccountMaskedType objItem = (bankAccountMaskedType)response.profile.paymentProfiles[i].payment.Item;
                        saveCardView.cardNumber = objItem.accountNumber;
                        saveCardView.cardType = objItem.bankName;
                        saveCardView.customerPaymentProfileId = response.profile.paymentProfiles[i].customerPaymentProfileId;
                    }
                    lstsaveCards.Add(saveCardView);
                }


            }
            else if (response != null)
            {
                Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                                  response.messages.message[0].text);
            }

            return lstsaveCards;
        }
        public string CreateCust(CardInfoViewModel data)
        {
            //Console.WriteLine("Create Customer Profile Sample");

            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = data.ANET_ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = data.ANET_ApiTransactionKey,
            };

            customerProfileType customerProfile = new customerProfileType();
            customerProfile.merchantCustomerId = data.CustomerId;
            customerProfile.email = data.email;

            var request = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

            // instantiate the controller that will call the service
            var controller = new createCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            createCustomerProfileResponse response = controller.GetApiResponse();

            // validate response 
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        //Console.WriteLine("Success!");
                        //Console.WriteLine("Customer Profile ID: " + );
                        return response.customerProfileId;
                    }
                }
                else
                {
                    Console.WriteLine("Customer Profile Creation Failed.");
                    Console.WriteLine("Error Code: " + response.messages.message[0].code);
                    Console.WriteLine("Error message: " + response.messages.message[0].text);
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    Console.WriteLine("Customer Profile Creation Failed.");
                    Console.WriteLine("Error Code: " + response.messages.message[0].code);
                    Console.WriteLine("Error message: " + response.messages.message[0].text);
                }
                else
                {
                    Console.WriteLine("Null Response.");
                }
            }

            return string.Empty;
        }
        public PaymentResponse CustTransection(CardInfoViewModel data, string customerPaymentProfileId)
        {
            PaymentResponse objPaymentRes = new PaymentResponse();
            try
            {
                //String ApiLoginID = "5z2fSW5G"; String ApiTransactionKey = "66JWg8eyL2Z78Sfs";
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = data.ANET_ApiLoginID,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = data.ANET_ApiTransactionKey,
                };
                objPaymentRes.cutomerProfileId = data.customerProfile_Id;
                objPaymentRes.transAmount = data.Amount;

                // Use CIM to create the profile we're going to charge
                var customerProfileId = data.customerProfile_Id;//"1960337182";
                var paymentProfileId = customerPaymentProfileId;// "1976458349";

                //create a customer payment profile
                customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
                profileToCharge.customerProfileId = customerProfileId;
                profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = paymentProfileId };


                var transactionRequestType = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                    amount = Convert.ToDecimal(data.Amount),
                    profile = profileToCharge,
                    poNumber = data.InvoiceNumber,
                    //transactionSettings = itemArr.ToArray(),
                    shipTo = new nameAndAddressType
                    {
                        firstName = data.shippingFName,
                        lastName = data.shippingLName,
                        address = data.shippingAddrLine1 + ' ' + data.shippingAddressLine2,
                        city = data.shippingCity,
                        company = data.shippingCompany,
                        state = data.shippingState,
                        country = data.shippingCountry,
                        zip = data.shippingZipCode

                    },
                    //billTo = new customerAddressType
                    //{
                    //    firstName = data.billingFName,
                    //    lastName = data.billingLName,
                    //    address = $"{data.billingAddrLine1} {data.billingAddressLine2}",
                    //    city = data.billingCity,
                    //    company = data.billingCompany,
                    //    state = data.billingState,
                    //    country = data.billingCountry,
                    //    zip = data.billingZipCode,
                    //    email = data.email,
                    //    faxNumber = !string.IsNullOrEmpty(data.billingFax) ? data.billingFax : string.Empty,
                    //    phoneNumber = data.billingPhno

                    //},
                    tax = new extendedAmountType
                    {
                        amount = Convert.ToDecimal(data.taxAmount),
                        name = "",
                        description = ""
                    }
                    ,
                   
                    shipping = new extendedAmountType
                    {
                        amount = Convert.ToDecimal(data.shipping),
                        name = "",
                        description = ""
                    },

                };
                var createRequest = new createTransactionRequest
                {
                    //refId = RefId,
                    transactionRequest = transactionRequestType,
                };
                //create controller, execute and get response
                var createController = new createTransactionController(createRequest);
                createController.Execute();
                var response = createController.GetApiResponse();

                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {

                        if (response.transactionResponse.messages != null)
                        {
                            //Console.WriteLine("Successfully created transaction with Transaction ID: " + response.transactionResponse.transId);
                            //Console.WriteLine("Response Code: " + response.transactionResponse.responseCode);
                            //Console.WriteLine("Message Code: " + response.transactionResponse.messages[0].code);
                            //Console.WriteLine("Description: " + response.transactionResponse.messages[0].description);
                            //Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);

                            objPaymentRes.statusCode = "200";
                            objPaymentRes.message = response.transactionResponse.messages[0].description;
                            objPaymentRes.transId = response.transactionResponse.transId;
                            objPaymentRes.transactionResponse = response.transactionResponse;
                        }
                        else
                        {
                            //Console.WriteLine("Failed Transaction.");
                            if (response.transactionResponse.errors != null)
                            {
                                //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                                //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);


                                objPaymentRes.statusCode = "400";
                                objPaymentRes.message = response.transactionResponse.errors[0].errorText;
                                objPaymentRes.transId = response.transactionResponse.transId;
                                objPaymentRes.transactionResponse = response.transactionResponse;
                            }
                        }
                    }
                    else
                    {
                        // Console.WriteLine("Failed Transaction.");
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                            objPaymentRes.statusCode = "400";
                            objPaymentRes.message = response.transactionResponse.errors[0].errorText;
                            objPaymentRes.transId = response.transactionResponse.transId;

                            objPaymentRes.transactionResponse = response.transactionResponse;
                        }
                        else
                        {
                            //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                            //Console.WriteLine("Error message: " + response.messages.message[0].text);

                            objPaymentRes.statusCode = "400";
                            objPaymentRes.message = response.messages.message[0].text;
                            objPaymentRes.transId = string.Empty;
                            objPaymentRes.transactionResponse = new transactionResponse();
                        }
                    }
                }
                else
                {
                    // Console.WriteLine("Null Response.");


                    objPaymentRes.statusCode = "400";
                    objPaymentRes.message = "Null Response";
                    objPaymentRes.transId = string.Empty;

                    objPaymentRes.transactionResponse = new transactionResponse();
                }
            }
            catch
            {

            }
            return objPaymentRes;
        }

        public PaymentResponse PaymentMethod(CardInfoViewModel data)
        {
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
            PaymentResponse objPaymentRes = new PaymentResponse();
            objPaymentRes.transAmount = data.Amount;
            objPaymentRes.aTranId = data.aTranId;
            objPaymentRes.cutomerProfileId = data.customerProfile_Id;
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = data.ANET_ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = data.ANET_ApiTransactionKey,
            };

            if (data.addnewcard.CCAddCard)
            {
                var creditCard = new creditCardType
                {
                    cardNumber = data.addnewcard.x_card_num,
                    expirationDate = data.addnewcard.x_exp_code
                };

                //standard api call to retrieve response
                var paymentType = new paymentType { Item = creditCard };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),    // authorize only
                    amount = data.Amount,
                    payment = paymentType,
                    poNumber = data.InvoiceNumber,
                    shipTo = new nameAndAddressType
                    {
                        firstName = data.shippingFName,
                        lastName = data.shippingLName,
                        address = data.shippingAddrLine1 + ' ' + data.shippingAddressLine2,
                        city = data.shippingCity,
                        company = data.shippingCompany,
                        state = data.shippingState,
                        country = data.shippingCountry,
                        zip = data.shippingZipCode

                    },
                    billTo = new customerAddressType
                    {
                        firstName = data.billingFName,
                        lastName = data.billingLName,
                        address = $"{data.billingAddrLine1} {data.billingAddressLine2}",
                        city = data.billingCity,
                        company = data.billingCompany,
                        state = data.billingState,
                        country = data.billingCountry,
                        zip = data.billingZipCode,
                        email = data.email,
                        faxNumber = !string.IsNullOrEmpty(data.billingFax) ? data.billingFax : string.Empty,
                        phoneNumber = data.billingPhno

                    },
                    tax = new extendedAmountType
                    {
                        amount = Convert.ToDecimal(data.taxAmount),
                        name = "",
                        description = ""
                    }
                    ,
                    duty = new extendedAmountType
                    {
                        amount = decimal.Parse("0.00"),
                        name = "",
                        description = ""
                    },
                    shipping = new extendedAmountType
                    {
                        amount = Convert.ToDecimal(data.shipping),
                        name = "",
                        description = ""
                    },


                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the controller that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (data.addnewcard.addPaycheck)
                        {
                            cardAdd(data);
                        }
                        if (response.transactionResponse.messages != null)
                        {
                            //Console.WriteLine("Successfully created transaction with Transaction ID: " + response.transactionResponse.transId);
                            //Console.WriteLine("Response Code: " + response.transactionResponse.responseCode);
                            //Console.WriteLine("Message Code: " + response.transactionResponse.messages[0].code);
                            //Console.WriteLine("Description: " + response.transactionResponse.messages[0].description);
                            //Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);

                            objPaymentRes.statusCode = "200";
                            objPaymentRes.message = response.transactionResponse.messages[0].description;
                            objPaymentRes.transId = response.transactionResponse.transId;
                            objPaymentRes.transactionResponse = response.transactionResponse;
                        }
                        else
                        {
                            //Console.WriteLine("Failed Transaction.");
                            if (response.transactionResponse.errors != null)
                            {
                                //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                                //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);


                                objPaymentRes.statusCode = "400";
                                objPaymentRes.message = response.transactionResponse.errors[0].errorText;
                                objPaymentRes.transId = response.transactionResponse.transId;
                                objPaymentRes.transactionResponse = response.transactionResponse;
                            }
                        }
                    }
                    else
                    {
                        // Console.WriteLine("Failed Transaction.");
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                            objPaymentRes.statusCode = "400";
                            objPaymentRes.message = response.transactionResponse.errors[0].errorText;
                            objPaymentRes.transId = response.transactionResponse.transId;

                            objPaymentRes.transactionResponse = response.transactionResponse;
                        }
                        else
                        {
                            //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                            //Console.WriteLine("Error message: " + response.messages.message[0].text);

                            objPaymentRes.statusCode = "400";
                            objPaymentRes.message = response.messages.message[0].text;
                            objPaymentRes.transId = string.Empty;
                            objPaymentRes.cutomerProfileId = data.customerProfile_Id;
                            objPaymentRes.transactionResponse = new transactionResponse();
                        }
                    }
                }
                else
                {
                    // Console.WriteLine("Null Response.");
                    var responseError = controller.GetErrorResponse();

                    if (responseError.messages != null)
                    {

                        objPaymentRes.statusCode = "400";
                        objPaymentRes.message = responseError.messages.message.Select(i => i.text).FirstOrDefault();
                        objPaymentRes.transId = string.Empty;

                        objPaymentRes.transactionResponse = new transactionResponse();

                    }
                    else
                    {
                        objPaymentRes.statusCode = "400";
                        objPaymentRes.message = "Null Response";
                        objPaymentRes.transId = string.Empty;

                        objPaymentRes.transactionResponse = new transactionResponse();
                    }

                }

                // return View("Payment", data);
            }
            else if (data.addnewcard.BankAddCard)
            {
                var bankAccount = new bankAccountType
                {
                    accountNumber = data.addnewcard.x_name_account_num,
                    routingNumber = data.addnewcard.x_aba_rout_num,
                    accountType = bankAccountTypeEnum.savings,
                    echeckType = echeckTypeEnum.WEB,
                    nameOnAccount = data.addnewcard.x_name_on_account,
                    bankName = data.addnewcard.x_bank_name
                    
                };

                paymentType echeck = new paymentType { Item = bankAccount };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),    // authorize only
                    amount = data.Amount,
                    payment = echeck
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the controller that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            if (data.addnewcard.addPaycheck)
                            {
                                cardAdd(data);
                            }
                            //Console.WriteLine("Successfully created transaction with Transaction ID: " + response.transactionResponse.transId);
                            //Console.WriteLine("Response Code: " + response.transactionResponse.responseCode);
                            //Console.WriteLine("Message Code: " + response.transactionResponse.messages[0].code);
                            //Console.WriteLine("Description: " + response.transactionResponse.messages[0].description);
                            //Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                            objPaymentRes.aTranId = data.aTranId;
                            objPaymentRes.statusCode = response.transactionResponse.responseCode;
                            objPaymentRes.message = response.transactionResponse.messages[0].description;
                            objPaymentRes.transId = response.transactionResponse.transId;
                            objPaymentRes.cutomerProfileId = data.customerProfile_Id;
                            objPaymentRes.transactionResponse = response.transactionResponse;
                        }
                        else
                        {
                            Console.WriteLine("Failed Transaction.");
                            if (response.transactionResponse.errors != null)
                            {
                                //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                                //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                                objPaymentRes.statusCode = "400";
                                objPaymentRes.message = response.transactionResponse.errors[0].errorText;
                                objPaymentRes.transId = response.transactionResponse.transId;
                                objPaymentRes.transactionResponse = response.transactionResponse;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed Transaction.");
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                            objPaymentRes.statusCode = "400";
                            objPaymentRes.message = response.transactionResponse.errors[0].errorText;
                            objPaymentRes.transId = response.transactionResponse.transId;

                            objPaymentRes.transactionResponse = response.transactionResponse;
                        }
                        else
                        {
                            //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                            //Console.WriteLine("Error message: " + response.messages.message[0].text);

                            objPaymentRes.statusCode = "400";
                            objPaymentRes.message = response.messages.message[0].text;
                            objPaymentRes.transId = string.Empty;
                            objPaymentRes.transactionResponse = new transactionResponse();
                        }
                    }
                }
                else
                {

                    objPaymentRes.statusCode = "400";
                    objPaymentRes.message = "Null Response";
                    objPaymentRes.transId = string.Empty;
                    objPaymentRes.transactionResponse = new transactionResponse();
                }

                //return View("Payment", data);
            }
            else
            {
                foreach (var item in data.savecard)
                {
                    if (item.CCDynamic)
                    {
                        CustTransection(data, item.customerPaymentProfileId);
                    }
                }
                //return View("Payment", data);
            }

            return objPaymentRes;
        }

        public void cardAdd(CardInfoViewModel data)
        {
            //Console.WriteLine("Update customer profile sample");

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = data.ANET_ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = data.ANET_ApiTransactionKey,
            };
            if (data.addnewcard.CCAddCard)
            {
                var creditCard = new creditCardType
                {
                    cardNumber = data.addnewcard.x_card_num,
                    expirationDate = data.addnewcard.x_exp_code
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
                    Console.WriteLine(response.messages.message[0].text);
                }
                else if (response != null)
                {
                    Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                                      response.messages.message[0].text);
                }
            }
            else if (data.addnewcard.BankAddCard)
            {
                var bankAccount = new bankAccountType
                {
                    accountNumber = data.addnewcard.x_name_account_num,
                    routingNumber = data.addnewcard.x_aba_rout_num,
                    accountType = bankAccountTypeEnum.checking,
                    echeckType = echeckTypeEnum.WEB,
                    nameOnAccount = data.addnewcard.x_name_on_account,
                    bankName = data.addnewcard.x_bank_name
                };

                paymentType echeck = new paymentType { Item = bankAccount };


                customerPaymentProfileExType ccPaymentProfile = new customerPaymentProfileExType();
                ccPaymentProfile.payment = echeck;

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
                    Console.WriteLine(response.messages.message[0].text);
                }
                else if (response != null)
                {
                    Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                                      response.messages.message[0].text);
                }
            }
        }

        public bool PostPaymentResponse(PaymentResponse resPonse, int reqId)
        {
            bool IsSuccess = false;
            try
            {
                Uri requestUri = new Uri(ConfigurationManager.AppSettings["PostPaymentInfoUrl"]);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri) as HttpWebRequest;
                string responpayment = $"postData={JsonConvert.SerializeObject(resPonse)}";
                byte[] requestBytes = System.Text.Encoding.UTF8.GetBytes(responpayment);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentLength = 0;
                request.ContentLength = requestBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
                HttpWebResponse responseAPI = (HttpWebResponse)request.GetResponse();
                Stream stream = responseAPI.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                var getResponse = reader.ReadToEnd();

                SaveResponse(resPonse, getResponse, reqId);

                return IsSuccess = true;
            }
            catch
            {

            }
            return IsSuccess;
        }
        private void SaveResponse(PaymentResponse resPonse, string aturianResponse, int reqId)
        {
            DbProvider _db = new DbProvider();
            try
            {
                _db.AddParameter("@requestId", reqId);
                _db.AddParameter("@transId", resPonse.transId);
                _db.AddParameter("@transResponse", JsonConvert.SerializeObject(resPonse));
                _db.AddParameter("@transStatus", resPonse.statusCode);
                _db.AddParameter("@message", resPonse.message);
                _db.AddParameter("@aturianResponse", aturianResponse);
                _db.ExecuteDataReader("SavePaymentResponse", CommandType.StoredProcedure);

            }
            catch
            {

            }
            finally
            {

            }
        }
    }
}