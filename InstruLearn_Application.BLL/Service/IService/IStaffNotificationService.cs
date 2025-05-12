using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IStaffNotificationService
    {
        Task<ResponseDTO> GetAllTeacherChangeRequestsAsync();
        Task<ResponseDTO> MarkNotificationAsReadAsync(int notificationId);
        Task<ResponseDTO> MarkNotificationAsResolvedAsync(int notificationId);
        Task<ResponseDTO> GetTeacherChangeRequestLearningRegistrationsAsync();
        Task<ResponseDTO> GetTeacherChangeRequestLearningRegistrationByIdAsync(int learningRegisId);
        Task<ResponseDTO> ChangeTeacherForLearningRegistrationAsync(int notificationId, int learningRegisId, int newTeacherId, string changeReason);
        Task<ResponseDTO> GetTeacherNotificationsAsync(int teacherId);
    }
}
