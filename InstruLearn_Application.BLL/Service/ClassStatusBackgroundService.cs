using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ClassStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClassStatusBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public ClassStatusBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ClassStatusBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int checkIntervalHours = _configuration.GetValue<int>("ClassStatus:CheckIntervalHours", 24);

            _logger.LogInformation("Running initial class status check at startup");
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await UpdateAllClassStatusesAsync(unitOfWork);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var targetHour = _configuration.GetValue<int>("ClassStatus:CheckHour", 1);
                    var nextRun = now.Hour >= targetHour
                        ? now.Date.AddDays(1).AddHours(targetHour)
                        : now.Date.AddHours(targetHour);

                    var delay = nextRun - now;

                    _logger.LogInformation("Class status check scheduled for: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        await UpdateAllClassStatusesAsync(unitOfWork);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred while updating class statuses");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }

        private async Task UpdateAllClassStatusesAsync(IUnitOfWork unitOfWork)
        {
            try
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                var classes = await unitOfWork.ClassRepository.GetAllAsync();
                var updatedClasses = 0;

                foreach (var classEntity in classes)
                {
                    var classDays = await unitOfWork.ClassDayRepository.GetQuery()
                        .Where(cd => cd.ClassId == classEntity.ClassId)
                        .ToListAsync();

                    if (classDays == null || !classDays.Any())
                        continue;

                    var endDate = DateTimeHelper.CalculateEndDate(
                        classEntity.StartDate,
                        classEntity.totalDays,
                        classDays.Select(cd => cd.Day).ToList());

                    ClassStatus newStatus;
                    ClassStatus oldStatus = classEntity.Status;

                    if (classEntity.TestDay == today)
                    {
                        newStatus = ClassStatus.OnTestDay;
                    }
                    else if (oldStatus == ClassStatus.OnTestDay && classEntity.StartDate > today)
                    {
                        newStatus = ClassStatus.OnTestDay;
                    }
                    else if (classEntity.StartDate > today)
                    {
                        newStatus = ClassStatus.Scheduled;
                    }
                    else if (classEntity.StartDate <= today && endDate >= today)
                    {
                        newStatus = ClassStatus.Ongoing;
                    }
                    else
                    {
                        newStatus = ClassStatus.Completed;
                    }

                    if (oldStatus != newStatus)
                    {
                        classEntity.Status = newStatus;
                        await unitOfWork.ClassRepository.UpdateAsync(classEntity);
                        updatedClasses++;

                        _logger.LogInformation("Updated class {ClassId} status from {OldStatus} to {NewStatus}",
                            classEntity.ClassId, oldStatus, newStatus);

                        if (oldStatus == ClassStatus.Scheduled && newStatus == ClassStatus.Ongoing)
                        {
                            await CreateTemporaryCertificatesForClass(unitOfWork, classEntity);
                        }
                    }
                }

                if (updatedClasses > 0)
                {
                    await unitOfWork.SaveChangeAsync();
                    _logger.LogInformation("Updated status for {Count} classes", updatedClasses);
                }
                else
                {
                    _logger.LogInformation("No class status updates needed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class statuses");
            }
        }

        private async Task CreateTemporaryCertificatesForClass(IUnitOfWork unitOfWork, Class classEntity)
        {
            try
            {
                var learners = await unitOfWork.dbContext.Learner_Classes
                    .Where(lc => lc.ClassId == classEntity.ClassId)
                    .Select(lc => lc.LearnerId)
                    .ToListAsync();

                if (!learners.Any())
                {
                    _logger.LogInformation("No learners found in class {ClassId} - no certificates to create", classEntity.ClassId);
                    return;
                }

                _logger.LogInformation("Creating temporary certificates for {Count} learners in class {ClassId}",
                    learners.Count, classEntity.ClassId);

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

                foreach (var learnerId in learners)
                {
                    var existingCertificates = await unitOfWork.CertificationRepository.GetByLearnerIdAsync(learnerId);
                    bool hasCertification = existingCertificates.Any(c =>
                        c.CertificationType == CertificationType.CenterLearning &&
                        c.CertificationName != null &&
                        c.CertificationName.Contains(classEntity.ClassId.ToString()));

                    if (hasCertification)
                    {
                        _logger.LogInformation("Learner {LearnerId} already has a certificate for class {ClassId}", learnerId, classEntity.ClassId);
                        continue;
                    }

                    var certification = new Certification
                    {
                        LearnerId = learnerId,
                        CertificationType = CertificationType.CenterLearning,
                        CertificationName = $"[TEMPORARY] Center Learning Certificate - {classEntity.ClassName} (Class ID: {classEntity.ClassId})",
                        TeacherName = teacherName,
                        Subject = majorName,
                        IssueDate = DateTime.Now
                    };

                    await unitOfWork.CertificationRepository.AddAsync(certification);
                    _logger.LogInformation("Created temporary certificate for learner {LearnerId} in class {ClassId}", learnerId, classEntity.ClassId);

                    var learner = await unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                    var staffNotification = new StaffNotification
                    {
                        Title = "Temporary Certificate Created",
                        Message = $"Learner {learner?.FullName ?? "Unknown"} (ID: {learnerId}) received a temporary certificate for class {classEntity.ClassName} (ID: {classEntity.ClassId}). Verify 75% attendance before finalizing certificate.",
                        LearnerId = learnerId,
                        CreatedAt = DateTime.Now.AddDays(classEntity.totalDays / 2),
                        Status = NotificationStatus.Unread,
                        Type = NotificationType.Certificate
                    };

                    await unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                }

                await unitOfWork.SaveChangeAsync();
                _logger.LogInformation("Successfully created temporary certificates for class {ClassId}", classEntity.ClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating temporary certificates for class {ClassId}", classEntity.ClassId);
            }
        }
    }
}
