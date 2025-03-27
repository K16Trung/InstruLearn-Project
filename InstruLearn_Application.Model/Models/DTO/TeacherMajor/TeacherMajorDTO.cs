using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.Major;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherMajor
{
    public class TeacherMajorDTO
    {
        public int TeacherMajorId { get; set; }
        public TeacherMajorDetailDTO teacher { get; set; }
        public TeacherMajorStatus Status { get; set; }
    }
}
