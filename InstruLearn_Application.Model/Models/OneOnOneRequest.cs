using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class OneOnOneRequest
    {
        [Key]
        public int RequestId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public TimeOnly TimeStart { get; set; }
        public int NumberOfSessions { get; set; }
        public OneOnOneRequestStatus Status { get; set; }
        public ICollection<OneOnOneRequestDays> OneOnOneRequestDays { get; set; }
        public ICollection<OneOnOneRequestTests> OneOnOneRequestTests { get; set; }
        public ICollection<OneOnOneSchedules> OneOnOneSchedules { get; set; }
    }
}
