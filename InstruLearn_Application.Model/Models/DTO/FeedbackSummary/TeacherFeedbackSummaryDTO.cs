using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.FeedbackSummary
{
    public class TeacherFeedbackSummaryDTO
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int TotalFeedbacks { get; set; }
        public double OverallAverageRating { get; set; }
        public Dictionary<string, double> CategoryAverages { get; set; }
        public List<QuestionSummaryDTO> QuestionSummaries { get; set; }
    }
}
