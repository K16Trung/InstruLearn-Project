using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LevelFeedbackCriterion
{
    public class UpdateLevelFeedbackCriterionDTO
    {
        public int CriterionId { get; set; }
        public string GradeCategory { get; set; }
        public decimal Weight { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
