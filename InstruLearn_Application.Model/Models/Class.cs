using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public int MajorId { get; set; }
        public int? LevelId { get; set; }
        public string ClassName { get; set; }
        public DateOnly StartDate { get; set; }
        public TimeOnly ClassTime { get; set; }
        public DateOnly TestDay { get; set; }
        public int MaxStudents { get; set; }
        public int totalDays { get; set; }
        public ClassStatus Status { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        // Navigation properties
        public Teacher Teacher { get; set; }
        public Major Major { get; set; }
        public LevelAssigned? Level { get; set; }
        public ICollection<ClassDay> ClassDays { get; set; }
        public ICollection<Learning_Registration> Learning_Registration { get; set; }
        public ICollection<Schedules> Schedules { get; set; }
        public ICollection<Learner_class> Learner_Classes { get; set; }
    }
}
