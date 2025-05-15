using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class StaffNotificationRepository : GenericRepository<StaffNotification>, IStaffNotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public StaffNotificationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<StaffNotification>> GetUnreadNotificationsAsync()
        {
            return await _context.StaffNotifications
                .Where(n => n.Status == NotificationStatus.Unread)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StaffNotification>> GetTeacherChangeRequestsAsync()
        {
            return await _context.StaffNotifications
                .Where(n => n.Type == NotificationType.TeacherChangeRequest)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StaffNotification>> GetContinueWithTeacherChangeRequestsAsync()
        {
            return await _context.StaffNotifications
                .Include(n => n.Learner)
                .Include(n => n.LearningRegistration)
                .Where(n => n.Type == NotificationType.TeacherChangeRequest)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.StaffNotifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.Status = NotificationStatus.Read;
                await UpdateAsync(notification);
            }
        }

        public async Task MarkAsResolvedAsync(int notificationId)
        {
            var notification = await _context.StaffNotifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.Status = NotificationStatus.Resolved;
                await UpdateAsync(notification);
            }
        }

        public async Task<List<StaffNotification>> GetNotificationsByTeacherIdAsync(int teacherId, NotificationType[] notificationTypes)
        {
            var directNotifications = await _context.StaffNotifications
                .Where(n => (n.Message.Contains($"teacher {teacherId}") ||
                            n.Title.Contains($"Teacher {teacherId}") ||
                            n.Title.Contains($"teacher {teacherId}")) &&
                            notificationTypes.Contains(n.Type) &&
                            n.Status != NotificationStatus.Resolved)
                .ToListAsync();

            var teacherRegistrationNotifications = await _context.StaffNotifications
                .Include(n => n.LearningRegistration)
                .Where(n => n.LearningRegistration.TeacherId == teacherId &&
                           notificationTypes.Contains(n.Type) &&
                           n.Status != NotificationStatus.Resolved)
                .ToListAsync();

            var classNotifications = await _context.StaffNotifications
                .Include(n => n.LearningRegistration)
                .Where(n => n.Type == NotificationType.ClassFeedback &&
                           notificationTypes.Contains(n.Type) &&
                           n.Status != NotificationStatus.Resolved &&
                           n.Message.Contains($"teacher {teacherId}"))
                .ToListAsync();

            var allNotifications = new List<StaffNotification>();
            allNotifications.AddRange(directNotifications);
            allNotifications.AddRange(teacherRegistrationNotifications);
            allNotifications.AddRange(classNotifications);

            return allNotifications.GroupBy(n => n.NotificationId)
                                  .Select(g => g.First())
                                  .ToList();
        }

        public async Task<List<StaffNotification>> GetNotificationsByLearnerIdAsync(int learnerId)
        {
            return await _context.StaffNotifications
                .Include(n => n.Learner)
                .Include(n => n.LearningRegistration)
                .Where(n => n.LearnerId == learnerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        private class StaffNotificationComparer : IEqualityComparer<StaffNotification>
        {
            public bool Equals(StaffNotification x, StaffNotification y)
            {
                return x.NotificationId == y.NotificationId;
            }

            public int GetHashCode(StaffNotification obj)
            {
                return obj.NotificationId.GetHashCode();
            }
        }

    }
}
