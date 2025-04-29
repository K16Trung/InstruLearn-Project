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
    }
}
