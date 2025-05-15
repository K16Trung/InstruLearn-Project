using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IFeedbackNotificationService
    {
        Task<ResponseDTO> CheckLearnerFeedbackNotificationsAsync(int learnerId);
        Task<ResponseDTO> ProcessFeedbackCompletionAsync(int feedbackId, bool continueStudying);
        Task<ResponseDTO> AutoCheckAndCreateFeedbackNotificationsAsync();
        Task<ResponseDTO> CheckAndUpdateLearnerProgressAsync();
        Task<ResponseDTO> CheckForExpiredFeedbacksAsync();
        Task<ResponseDTO> CheckForClassLastDayFeedbacksAsync();
        Task SendTestFeedbackEmailNotification(string email, string learnerName, int feedbackId, string teacherName, decimal remainingPayment);
    }
}
