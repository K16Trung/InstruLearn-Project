using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learning_Registration_Type
    {
        [Key]
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public ICollection<Learning_Registration> Learning_Registrations { get; set; }
    }
}
