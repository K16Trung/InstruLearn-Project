using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.StaffNotification
{
    public class StaffNotificationDTO
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int? LearningRegisId { get; set; }
        public int? LearnerId { get; set; }
        public string LearnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationStatus Status { get; set; }
        public NotificationType Type { get; set; }
    }
}
