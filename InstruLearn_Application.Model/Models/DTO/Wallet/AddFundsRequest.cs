using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Wallet
{
    public class AddFundsRequest
    {
        public int LearnerId { get; set; }
        public decimal Amount { get; set; }
    }
}
