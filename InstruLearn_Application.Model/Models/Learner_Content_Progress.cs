using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learner_Content_Progress
    {
        [Key]
        public int ProgressId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int ItemId { get; set; }
        public Course_Content_Item ContentItem { get; set; }
        public double WatchTimeInSeconds { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastAccessDate { get; set; } = DateTime.Now;
    }
}
