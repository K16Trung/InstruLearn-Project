using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class ChangeClassDTO
    {
        public int LearnerId { get; set; }
        public int ClassId { get; set; }  // The new class ID
        public string? Reason { get; set; }
    }
}
