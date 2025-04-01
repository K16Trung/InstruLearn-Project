using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Wallet
{
    public class PayOSWebhookRequest
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
    }
}
