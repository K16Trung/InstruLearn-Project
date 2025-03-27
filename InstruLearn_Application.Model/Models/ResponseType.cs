using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class ResponseType
    {
        [Key]
        public int ResponseTypeId { get; set; }
        public string ResponseTypeName { get; set; }

        //Navigation property
        public ICollection<Response> Responses { get; set; }
    }
}
