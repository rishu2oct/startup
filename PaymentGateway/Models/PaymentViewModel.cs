using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaymentGateway.Models
{
    public class PaymentViewModel
    {
        public class Carddata
        {
            public string CardNum { get; set; }
            public string ExpDate { get; set; }
            public string CP_ID { get; set; }
            public string CPP_Id { get; set; }
            public string amount { get; set; }
        }
        public class CustomerModel
        {
            public string CustomerId { get; set; }
            public string email { get; set; }
        }
        public class CustomerUpdateModel
        {
            public string CustomerId { get; set; }
            public string email { get; set; }
        }
        public class AddPaymentMethodModel
        {
            public string PaymentCardInfo { get; set; }
            public string CardNumber { get; set; }
            public string ExpCode { get; set; }
            public string CVVNumber { get; set; }
            public string BankName { get; set; }
            public string NameOnAccount { get; set; }
            public string AccountNumber { get; set; }
            public string ABARoutingNumber { get; set; }
            public int BankAccountType { get; set; }
            public string customerProfileId { get; set; }
            public bool checkAddcard { get; set; }
            public decimal Amount { get; set; }
        }

        public class CardInfoViewModel
        {
            public CardInfoViewModel()
            {
                savecard = new List<SaveCardViewModel>();
                addnewcard = new AddNewCardViewModel();
                addPayment = new AddPaymentProfile();
            }
            public string CustomerId { get; set; }
            public int reqId { get; set; }
            public string email { get; set; }
            public string customerProfile_Id { get; set; }
            public string OderNumber { get; set; }
            public string InvoiceNumber { get; set; }
            [DisplayFormat(DataFormatString = "{0:n0}")]
            public decimal Amount { get; set; }
            public string ANET_ApiLoginID { get; set; }
            public string ANET_ApiTransactionKey { get; set; }
            public string Environment { get; set; }
            public string currency { get; set; }
            public string PostType { get; set; }
            public string aTranId { get; set; }

            public double taxAmount { get; set; }
            public double shipping { get; set; }
            public double serviceChargePercentage { get; set; }
            public int paymentOption { get; set; }
            public string docType { get; set; }
            public string customerPaymentProfileId { get; set; }

            #region[shipping and billing info]
            public string billingFName { get; set; }
            public string billingLName { get; set; }
            public string billingCompany { get; set; }
            public string billingAddrLine1 { get; set; }
            public string billingAddressLine2 { get; set; }
            public string billingCity { get; set; }
            public string billingState { get; set; }
            public string billingCountry { get; set; }
            public string billingZipCode { get; set; }
            public string billingPhno { get; set; }
            public string billingFax { get; set; }
            public string shippingFName { get; set; }
            public string shippingLName { get; set; }
            public string shippingCompany { get; set; }
            public string shippingAddrLine1 { get; set; }
            public string shippingAddressLine2 { get; set; }
            public string shippingCity { get; set; }
            public string shippingState { get; set; }
            public string shippingCountry { get; set; }
            public string shippingZipCode { get; set; }
            public string shippingPhno { get; set; }
            public string shippingFax { get; set; }

            #endregion


            public List<SaveCardViewModel> savecard { get; set; } 
            public AddNewCardViewModel addnewcard { get; set; }
            public AddPaymentProfile addPayment { get; set; }
        }
        public class SaveCardViewModel
        {
            public bool CCDynamic { get; set; }
            public string cardType { get; set; }
            public string cardNumber { get; set; }
            public string expirationDate { get; set; }
            public string customerPaymentProfileId { get; set; }
        }
        public class AddNewCardViewModel
        {
            public bool CCAddCard { get; set; }
            public bool BankAddCard { get; set; }
            public bool SaveCard { get; set; }
            public string custumerCardId { get; set; }
            public string x_card_num { get; set; }
            public string x_exp_code { get; set; }
            public string x_cvv_num { get; set; }
            public string x_bank_name { get; set; }
            public string x_name_on_account { get; set; }
            public string x_name_account_num { get; set; }
            public string x_aba_rout_num { get; set; }
            public int x_bank_type { get; set; }
            public bool addPaycheck { get; set; }
            public bool isCardSave { get; set; }
        }
        public class AddPaymentProfile
        {
            public bool CCID { get; set; }
            public bool BAccount { get; set; }
            public string PayAdd_x_card_num { get; set; }
            public string PayAdd_exp_code { get; set; }
            public string PayAdd_cvv_num { get; set; }
            public string PayAdd_bank_name { get; set; }
            public string PayAdd_name_on_acc { get; set; }
            public string PayAdd_account_num { get; set; }
            public string ABA_Routing_num { get; set; }
            public int bank_account_type { get; set; }
        }
    }
}