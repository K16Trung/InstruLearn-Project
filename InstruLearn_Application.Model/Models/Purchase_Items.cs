using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Purchase_Items
    {
        [Key]
        public int PurchaseItemId { get; set; }
        public int PurchaseId { get; set; }
        public int CoursePackageId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public Purchase Purchase { get; set; }
        public Course_Package CoursePackage { get; set; }
    }
}
