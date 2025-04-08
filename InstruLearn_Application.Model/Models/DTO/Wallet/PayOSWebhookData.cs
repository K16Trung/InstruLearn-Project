using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Wallet
{
    public class PayOSWebhookData
    {
        public string TransactionId { get; set; }
        public long OrderCode { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }
}
