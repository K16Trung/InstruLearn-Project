using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int ResponseId { get; set; }
        public string ResponseName { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LevelPrice { get; set; }
        public DateOnly? StartDay { get; set; }
        public TimeOnly TimeStart { get; set; }
        public int TimeLearning { get; set; }
        public TimeOnly? TimeEnd { get; set; }
        public DateTime RequestDate { get; set; }
        public int NumberOfSession { get; set; }
        public string VideoUrl { get; set; }
        public string LearningRequest { get; set; }
        public List<string> LearningDays { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }
}
