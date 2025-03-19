using InstruLearn_Application.Model.Models.DTO.Course;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.PurchaseItem
{
    public class PurchaseItemDTO
    {
        public int PurchaseItemId { get; set; }
        public int CoursePackageId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public CourseDetailPurchaseDTO CoursePackage { get; set; }
    }
}
