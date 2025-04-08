using InstruLearn_Application.Model.Models.DTO.PayOSWebhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Wallet
{
    public class PayOSWebhookRequest
    {
        public PayOSWebhookData Data { get; set; }
        public string Signature { get; set; }
    }
}
