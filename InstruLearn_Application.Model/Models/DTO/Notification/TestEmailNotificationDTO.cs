using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Notification
{
    public class TestEmailNotificationDTO
    {
        public string Email { get; set; }
        public string LearnerName { get; set; }
        public int FeedbackId { get; set; } = 123;
        public string TeacherName { get; set; }
        public decimal RemainingPayment { get; set; } = 1000;
    }
}
