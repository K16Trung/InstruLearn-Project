using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Certification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CertificateCreationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CertificateCreationBackgroundService> _logger;

        public CertificateCreationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<CertificateCreationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Certificate Creation Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Certificate creation check running at: {time}", DateTimeOffset.Now);

                try
                {
                    await ProcessPendingCertificatesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing pending certificates");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task ProcessPendingCertificatesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var certificationService = scope.ServiceProvider.GetRequiredService<ICertificationService>();

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateTime todayDateTime = DateTime.Today;

            var pendingNotifications = await unitOfWork.StaffNotificationRepository.GetQuery()
                .Where(n => n.Type == NotificationType.Certificate &&
                          n.Status == NotificationStatus.Unread &&
                          n.Message.Contains("certificate") &&
                          n.Message.Contains("start date") &&
                          n.CreatedAt.Date <= todayDateTime)
                .ToListAsync(cancellationToken);

            _logger.LogInformation($"Found {pendingNotifications.Count()} pending certificate notifications to process");

            foreach (var notification in pendingNotifications)
            {
                if (!notification.LearnerId.HasValue)
                {
                    _logger.LogWarning($"Notification {notification.NotificationId} is missing LearnerId. Skipping.");
                    continue;
                }

                try
                {
                    int learnerId = notification.LearnerId.Value;

                    int classId = 0;
                    string message = notification.Message ?? "";
                    var classIdMatch = System.Text.RegularExpressions.Regex.Match(message, @"class.*?\(ID: (\d+)\)");
                    if (classIdMatch.Success && classIdMatch.Groups.Count > 1)
                    {
                        if (int.TryParse(classIdMatch.Groups[1].Value, out int parsedId))
                        {
                            classId = parsedId;
                        }
                    }

                    if (classId == 0)
                    {
                        _logger.LogWarning($"Could not extract class ID from notification {notification.NotificationId}. Skipping.");
                        continue;
                    }

                    string startDateStr = "";
                    var startDateMatch = System.Text.RegularExpressions.Regex.Match(message, @"start date (\d{1,2}/\d{1,2}/\d{4})");
                    if (startDateMatch.Success && startDateMatch.Groups.Count > 1)
                    {
                        startDateStr = startDateMatch.Groups[1].Value;
                        if (DateTime.TryParse(startDateStr, out DateTime scheduledDate))
                        {
                            if (scheduledDate.Date > todayDateTime)
                            {
                                _logger.LogInformation($"Certificate for class {classId} is scheduled for {scheduledDate.ToShortDateString()}, not today. Skipping.");
                                continue;
                            }
                        }
                    }

                    var learningRegistration = await unitOfWork.LearningRegisRepository.GetQuery()
                        .FirstOrDefaultAsync(lr => lr.LearnerId == learnerId && lr.ClassId == classId, cancellationToken);

                    if (learningRegistration == null)
                    {
                        _logger.LogWarning($"No learning registration found for learner {learnerId} in class {classId}. Skipping.");
                        notification.Status = NotificationStatus.Read;

                        await unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                        await unitOfWork.SaveChangeAsync();
                        continue;
                    }

                    var existingCertificates = await unitOfWork.CertificationRepository.GetByLearnerIdAsync(learnerId);
                    bool certificateExists = existingCertificates.Any(c =>
                        c.CertificationType == CertificationType.CenterLearning &&
                        c.CertificationName != null &&
                        c.CertificationName.Contains(classId.ToString()));

                    if (certificateExists)
                    {
                        _logger.LogInformation($"Certificate already exists for learner {learnerId} for class {classId}. Marking notification as read.");
                        notification.Status = NotificationStatus.Read;

                        await unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                        await unitOfWork.SaveChangeAsync();
                        continue;
                    }

                    var classEntity = await unitOfWork.ClassRepository.GetByIdAsync(classId);
                    var learner = await unitOfWork.LearnerRepository.GetByIdAsync(learnerId);

                    if (classEntity == null || learner == null)
                    {
                        _logger.LogWarning($"Class {classId} or learner {learnerId} not found. Skipping certificate creation.");
                        notification.Status = NotificationStatus.Read;

                        await unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                        await unitOfWork.SaveChangeAsync();
                        continue;
                    }

                    string teacherName = "Unknown Teacher";
                    if (classEntity.Teacher != null)
                    {
                        teacherName = classEntity.Teacher.Fullname;
                    }
                    else
                    {
                        var teacher = await unitOfWork.TeacherRepository.GetByIdAsync(classEntity.TeacherId);
                        if (teacher != null)
                        {
                            teacherName = teacher.Fullname;
                        }
                    }

                    string majorName = "Unknown Subject";
                    if (classEntity.Major != null)
                    {
                        majorName = classEntity.Major.MajorName;
                    }
                    else
                    {
                        var major = await unitOfWork.MajorRepository.GetByIdAsync(classEntity.MajorId);
                        if (major != null)
                        {
                            majorName = major.MajorName;
                        }
                    }

                    var createCertificationDTO = new CreateCertificationDTO
                    {
                        LearnerId = learnerId,
                        CertificationType = CertificationType.CenterLearning,
                        CertificationName = $"Center Learning Certificate - {classEntity.ClassName} (Class ID: {classId})",
                        TeacherName = teacherName,
                        Subject = majorName
                    };

                    var certResult = await certificationService.CreateCertificationAsync(createCertificationDTO);

                    if (certResult.IsSucceed)
                    {
                        _logger.LogInformation($"Certificate created successfully for learner {learnerId} in class {classId}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to create certificate: {certResult.Message}");
                    }

                    notification.Status = NotificationStatus.Read;
                    await unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                    await unitOfWork.SaveChangeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing notification {notification.NotificationId}");
                }
            }
        }
    }
}