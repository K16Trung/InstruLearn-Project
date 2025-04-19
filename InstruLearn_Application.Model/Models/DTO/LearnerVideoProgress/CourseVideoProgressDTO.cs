using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerVideoProgress
{
    public class CourseVideoProgressDTO
    {
        public int CoursePackageId { get; set; }
        public string CourseName { get; set; }
        public double TotalVideoDuration { get; set; }
        public double TotalWatchTime { get; set; }
        public double CompletionPercentage { get; set; }
    }
}
