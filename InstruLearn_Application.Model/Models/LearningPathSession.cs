using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LearningPathSession
    {
        [Key]
        public int LearningPathSessionId { get; set; }

        public int LearningRegisId { get; set; }

        [ForeignKey("LearningRegisId")]
        public Learning_Registration LearningRegistration { get; set; }

        public int SessionNumber { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
