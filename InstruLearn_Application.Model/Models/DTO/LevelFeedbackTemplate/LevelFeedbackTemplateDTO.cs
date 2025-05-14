using InstruLearn_Application.Model.Models.DTO.LevelFeedbackCriterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LevelFeedbackTemplate
{
    public class LevelFeedbackTemplateDTO
    {
        public int TemplateId { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string MajorName { get; set; }
        public string TemplateName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LevelFeedbackCriterionDTO> Criteria { get; set; }
    }
}
