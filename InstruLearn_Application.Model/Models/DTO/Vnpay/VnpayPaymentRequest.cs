using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Vnpay
{
    public class VnpayPaymentRequest
    {
        public string OrderType { get; set; } = "150000"; // Default order type
        public string OrderDescription { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int LearnerId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailureUrl { get; set; } = string.Empty;
    }
}
