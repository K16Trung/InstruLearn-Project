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
        public int ResponseTypeId { get; set; }
        public string ResponseTypeName { get; set; }
        public int ResponseId { get; set; }
        public string ResponseDescription { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LevelPrice { get; set; }
        public string SyllabusLink { get; set; }
        public DateOnly? StartDay { get; set; }
        public TimeOnly TimeStart { get; set; }
        public int TimeLearning { get; set; }
        public TimeOnly? TimeEnd { get; set; }
        public DateTime RequestDate { get; set; }
        public int NumberOfSession { get; set; }
        public string SelfAssessment { get; set; }
        public string VideoUrl { get; set; }
        public string LearningRequest { get; set; }
        public List<string> LearningDays { get; set; }
        public decimal? Price { get; set; }
        //public string? LearningPath { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RemainingAmount { get; set; }
        public string Status { get; set; }

        // New properties for payment deadline
        public DateTime? AcceptedDate { get; set; }
        public DateTime? PaymentDeadline { get; set; }
        public int? DaysRemaining => PaymentDeadline.HasValue ? (PaymentDeadline.Value.Date - DateTime.Now.Date).Days : null;
        public string PaymentStatus => Status == "Accepted" ? "Pending" :
                                      Status == "Fourty" ? "40% payment" :
                                      PaymentDeadline.HasValue && PaymentDeadline < DateTime.Now ? "Overdue" :
                                      "";
    }
}
