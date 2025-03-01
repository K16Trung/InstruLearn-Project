using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        public DateTime UpdateAt { get; set; }
        public ICollection<WalletTransaction> WalletTransactions { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
