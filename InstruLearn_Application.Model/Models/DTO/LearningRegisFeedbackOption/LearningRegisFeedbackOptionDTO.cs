using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackOption
{
    public class LearningRegisFeedbackOptionDTO
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public int Value { get; set; }
        public int DisplayOrder { get; set; }
    }
}
