using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Payment
{
    public class PaymentStatusRequest
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
    }
}
