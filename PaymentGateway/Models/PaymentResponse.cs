using AuthorizeNet.Api.Contracts.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentGateway.Models
{
    public class PaymentResponse
    {
        public PaymentResponse()
        {
            transactionResponse = new transactionResponse();
        }
        public string statusCode { get; set; }
        public string message { get; set; }
        public string aTranId { get; set; }
        public string transId { get; set; }
        public string cutomerProfileId { get; set; }

        public decimal transAmount { get; set; }
        public transactionResponse transactionResponse { get; set; }

    }
}