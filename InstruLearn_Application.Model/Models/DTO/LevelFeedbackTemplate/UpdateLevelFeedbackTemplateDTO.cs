using InstruLearn_Application.Model.Models.DTO.LevelFeedbackCriterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LevelFeedbackTemplate
{
    public class UpdateLevelFeedbackTemplateDTO
    {
        public string TemplateName { get; set; }
        public bool IsActive { get; set; }
        public List<UpdateLevelFeedbackCriterionDTO> Criteria { get; set; }
    }
}
