using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learning_Registration_Type
    {
        [Key]
        public int RegisTypeId { get; set; }
        public string RegisTypeName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal RegisPrice { get; set; }
        public ICollection<Learning_Registration> Learning_Registrations { get; set; }
    }
}
