using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learning_Registration
    {
        [Key]
        public int LearningRegisId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public int? ClassId { get; set; }
        public Class? Classes { get; set; }
        public int RegisTypeId { get; set; }
        public Learning_Registration_Type Learning_Registration_Type { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public int ResponseId { get; set; }
        public Response Response { get; set; }
        public int LevelId { get; set; }
        public LevelAssigned LevelAssigned { get; set; }
        public DateOnly? StartDay { get; set; }
        public TimeOnly TimeStart { get; set; }
        public int TimeLearning { get; set; }
        public string LearningRequest { get; set; }
        public DateTime RequestDate { get; set; }
        public LearningRegis Status { get; set; }
        public int NumberOfSession { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        public string VideoUrl { get; set; }

        // Navigation properties
        public ICollection<Test_Result> Test_Results { get; set; }
        public ICollection<LearningRegistrationDay> LearningRegistrationDay { get; set; }
        public ICollection<Schedules> Schedules { get; set; }

    }
}
