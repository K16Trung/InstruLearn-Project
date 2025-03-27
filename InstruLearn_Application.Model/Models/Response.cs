using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Response
    {
        [Key]
        public int ResponseId { get; set; }
        public int ResponseTypeId { get; set; }
        public ResponseType ResponseType { get; set; }
        public string ResponseName { get; set; }
        //Navigation properties
        public ICollection<Learning_Registration> Learning_Registrations { get; set; }
    }
}
