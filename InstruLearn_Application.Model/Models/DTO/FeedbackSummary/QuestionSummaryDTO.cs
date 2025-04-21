using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.FeedbackSummary
{
    public class QuestionSummaryDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; }
        public double AverageRating { get; set; }
        public List<OptionCountDTO> OptionCounts { get; set; }
    }
}
