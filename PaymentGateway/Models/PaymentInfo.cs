using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbProviderFactorie;
using System.Data;
using System.Data.Common;
namespace PaymentGateway.Models
{
    /// <summary>
    /// get payment response info api class 
    /// </summary>
    public class PaymentInfo
    {

        public int tranid { get; set; }
        public string tranidkey { get; set; }
        public double amount { get; set; }
        public string customeremail { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentSource { get; set; }
        public string ANET_ApiLoginID { get; set; }
        public string ANET_ApiTransactionKey { get; set; }
        public string CustomerProfileId { get; set; }
        public string env { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public string currency { get; set; }
        public string docType { get; set; }
        public double taxAmount { get; set; }
        public double shipping { get; set; }
        public string zipCode { get; set; }
        public string CVV { get; set; }
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


        public static int SaveRequest(PaymentInfo objInfo)
        {
            DbProvider _db = new DbProvider();
            int id = 0;
            try
            {

                _db.AddParameter("@aTransId", objInfo.tranidkey);
                _db.AddParameter("@CustomerEmail", objInfo.customeremail);
                _db.AddParameter("@OrderNumber", objInfo.OrderNumber);
                _db.AddParameter("@InvoiceNumber", objInfo.InvoiceNumber);
                _db.AddParameter("@PaymentSource", objInfo.PaymentSource);
                _db.AddParameter("@ANET_ApiLoginID", objInfo.ANET_ApiLoginID);
                _db.AddParameter("@ANET_ApiTransactionKey", objInfo.ANET_ApiTransactionKey);
                _db.AddParameter("@env", objInfo.env);
                _db.AddParameter("@StatusCode", objInfo.StatusCode);
                _db.AddParameter("@Massage", objInfo.Message);
                _db.AddParameter("@Amount", objInfo.amount);
                using (DbDataReader dr = _db.ExecuteDataReader("CreatePaymentRequest", CommandType.StoredProcedure))
                {
                    dr.Read();
                    if (dr.HasRows)
                    {
                        id = Convert.ToInt32(dr["Id"]);
                    }
                    dr.Close();
                };




            }
            catch
            {

            }
            finally
            {
                _db.Dispose();
            }
            return id;
        }

    }
}