using InstruLearn_Application.Model.Models.DTO.LevelFeedbackCriterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LevelFeedbackTemplate
{
    public class CreateLevelFeedbackTemplateDTO
    {
        public int LevelId { get; set; }
        public string TemplateName { get; set; }
        public List<CreateLevelFeedbackCriterionDTO> Criteria { get; set; }
    }
}
