using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Vnpay
{
    public class VnpayPaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderDescription { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;

        // Additional fields for direct VNPay parameters
        public string BankTranNo { get; set; } = string.Empty;
        public string PayDate { get; set; } = string.Empty;
        public string TmnCode { get; set; } = string.Empty;
        public string TransactionNo { get; set; } = string.Empty;
        public string TxnRef { get; set; } = string.Empty;
        public string SecureHash { get; set; } = string.Empty;
    }
}
