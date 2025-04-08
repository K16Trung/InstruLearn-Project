using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class WalletTransaction
    {
        [Key]
        public string TransactionId { get; set; }
        public long? OrderCode { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public ICollection<Payment> Payments { get; set; }
        [NotMapped]
        public decimal SignedAmount => TransactionType == TransactionType.Payment ? -Amount : Amount;
    }
}
