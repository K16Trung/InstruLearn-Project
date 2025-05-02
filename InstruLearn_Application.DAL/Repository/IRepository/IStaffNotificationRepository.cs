using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IStaffNotificationRepository : IGenericRepository<StaffNotification>
    {
        Task<List<StaffNotification>> GetUnreadNotificationsAsync();
        Task<List<StaffNotification>> GetTeacherChangeRequestsAsync();
        Task<List<StaffNotification>> GetContinueWithTeacherChangeRequestsAsync();
        Task<List<StaffNotification>> GetNotificationsByTeacherIdAsync(int teacherId, NotificationType[] notificationTypes);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAsResolvedAsync(int notificationId);
    }
}
