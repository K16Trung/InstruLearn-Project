using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class StaffNotification
    {
        [Key]
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int? LearningRegisId { get; set; }
        public int? LearnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationStatus Status { get; set; }
        public NotificationType Type { get; set; }

        // Navigation properties
        public virtual Learning_Registration LearningRegistration { get; set; }
        public virtual Learner Learner { get; set; }
    }
}
