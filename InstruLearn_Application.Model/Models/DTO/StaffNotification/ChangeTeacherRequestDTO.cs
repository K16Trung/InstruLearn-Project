using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.StaffNotification
{
    public class ChangeTeacherRequestDTO
    {
        [Required]
        public int NotificationId { get; set; }

        [Required]
        public int LearningRegisId { get; set; }

        [Required]
        public int NewTeacherId { get; set; }

        [StringLength(500)]
        public string? ChangeReason { get; set; }
    }
}
