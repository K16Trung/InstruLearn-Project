using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Payment
{
    public class CreatePaymentDTO
    {
        public int LearningRegisId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
