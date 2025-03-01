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
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; }
        public int? TransactionId { get; set; }
        public WalletTransaction? WalletTransaction { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentFor PaymentFor { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
