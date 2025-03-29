using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Purchase
{
    public class CreatePurchaseDTO
    {
        public int LearnerId { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
