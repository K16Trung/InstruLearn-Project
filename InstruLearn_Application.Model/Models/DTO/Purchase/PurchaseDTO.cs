﻿using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.PurchaseItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Purchase
{
    public class PurchaseDTO
    {
        public LearnerInfoDTO Learner { get; set; }
        public int PurchaseId { get; set; }
        public int LearnerId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public PurchaseStatus Status { get; set; }
        public List<PurchaseItemDTO> PurchaseItems { get; set; } = new List<PurchaseItemDTO>();
    }

    // New DTO specifically for limited learner info in the purchase context
    public class LearnerInfoDTO
    {
        public int LearnerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}