using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Notification
{
    public class NotificationDTO
    {
        //public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RecipientEmail { get; set; }
        public DateTime SentDate { get; set; }
        public NotificationType? NotificationType { get; set; }
        public NotificationStatus Status { get; set; }
        public int? LearningRegisId { get; set; }
        public string LearningRequest { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Deadline { get; set; }
        

    }
}
