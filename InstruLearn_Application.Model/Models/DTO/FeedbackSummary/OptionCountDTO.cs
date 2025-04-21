using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.FeedbackSummary
{
    public class OptionCountDTO
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
