using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.WalletTransaction
{
    public class WalletTransactionDTO
    {
        public string TransactionId { get; set; }
        public int WalletId { get; set; }
        public string LearnerFullname { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal SignedAmount { get; set; }
        public string PaymentType { get; set; }
    }
}
