using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerCourse
{
    public class ContentItemProgressDTO
    {
        public int ItemId { get; set; }
        public string ItemDes { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public bool IsLearned { get; set; }
        public double? DurationInSeconds { get; set; }
        public double WatchTimeInSeconds { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime? LastAccessDate { get; set; }
    }
}
