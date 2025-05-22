using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class LearnerClassEligibilityDTO
    {
        [Required]
        public int LearnerId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public bool IsEligible { get; set; }
    }
}
