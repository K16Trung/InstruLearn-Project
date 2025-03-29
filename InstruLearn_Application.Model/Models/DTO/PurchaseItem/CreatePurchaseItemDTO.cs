using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.PurchaseItem
{
    public class CreatePurchaseItemDTO
    {
        public int LearnerId { get; set; }
        public List<CoursePackageItem> CoursePackages { get; set; } = new List<CoursePackageItem>();
        public class CoursePackageItem
        {
            public int CoursePackageId { get; set; }
        }
    }
}
