using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegistration
{
    public class OneOnOneRegisDTO
    {
        public int LearningRegisId { get; set; }
        public int LearnerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int RegisTypeId { get; set; }
        public string RegisTypeName { get; set; }
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public DateTime RequestDate { get; set; }
        public int NumberOfSession { get; set; }
        public string VideoUrl { get; set; }
        public int? Score { get; set; }
        public string? LevelAssigned { get; set; }
        public string? Feedback { get; set; }
        public List<string> LearningDays { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }
}
