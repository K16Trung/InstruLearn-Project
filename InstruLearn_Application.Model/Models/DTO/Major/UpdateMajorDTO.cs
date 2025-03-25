using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Major
{
    public class UpdateMajorDTO
    {
        public string MajorName { get; set; }
        public MajorStatus Status { get; set; }
    }
}
