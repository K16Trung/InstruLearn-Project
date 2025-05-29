using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
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
    public class ClassFeedbackBackgroundService : BackgroundService
    {
        private readonly ILogger<ClassFeedbackBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public ClassFeedbackBackgroundService(
            ILogger<ClassFeedbackBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        /*protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int checkIntervalHours = _configuration.GetValue<int>("ClassFeedback:CheckIntervalHours", 24);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var targetHour = _configuration.GetValue<int>("ClassFeedback:CheckHour", 7);
                    var nextRun = now.Hour >= targetHour
                        ? now.Date.AddDays(1).AddHours(targetHour)
                        : now.Date.AddHours(targetHour);

                    var delay = nextRun - now;

                    _logger.LogInformation("Class feedback check scheduled for: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var feedbackNotificationService = scope.ServiceProvider.GetRequiredService<IFeedbackNotificationService>();
                        var result = await feedbackNotificationService.CheckForClassLastDayFeedbacksAsync();

                        if (result.IsSucceed && result.Data != null)
                        {
                            _logger.LogInformation("Created feedback forms for classes on their last day or recently ended: {Message}", result.Message);

                            if (result.Data is System.Collections.Generic.List<object> classesProcessed && classesProcessed.Count > 0)
                            {
                                var unitOfWork = scope.ServiceProvider.GetRequiredService<InstruLearn_Application.DAL.UoW.IUoW.IUnitOfWork>();

                                foreach (dynamic classInfo in classesProcessed)
                                {
                                    try
                                    {
                                        int classId = classInfo.ClassId;
                                        string className = classInfo.ClassName;
                                        int teacherId = classInfo.TeacherId;
                                        string teacherName = classInfo.TeacherName;
                                        int feedbacksCreated = classInfo.FeedbacksCreated;
                                        string levelName = classInfo.LevelName;
                                        string majorName = classInfo.MajorName;
                                        int templatesUsed = classInfo.TemplateId;
                                        string templateName = classInfo.TemplateName;
                                        bool isLastDay = classInfo.IsLastDay;
                                        bool isRecentlyEnded = classInfo.IsRecentlyEnded;
                                        DateOnly endDate = classInfo.EndDate;

                                        if (feedbacksCreated > 0)
                                        {
                                            string statusMessage = isLastDay
                                                ? "has reached its last day today"
                                                : $"ended on {endDate:dd/MM/yyyy} without complete feedback";

                                            var notification = new StaffNotification
                                            {
                                                Type = NotificationType.ClassFeedback,
                                                Status = NotificationStatus.Unread,
                                                CreatedAt = DateTime.Now,
                                                Title = $"Feedback Required for Class: {className}",
                                                Message = $"Please complete feedback forms for {feedbacksCreated} students in your {levelName} {majorName} class '{className}'. This class {statusMessage}. All feedback forms have been prepared with the template '{templateName}'."
                                            };

                                            await unitOfWork.StaffNotificationRepository.AddAsync(notification);
                                            _logger.LogInformation($"Created notification for teacher {teacherId} ({teacherName}) for class {classId} ({className}) that {statusMessage}");
                                        }
                                        else
                                        {
                                            _logger.LogInformation($"No new feedback forms were created for class {classId} ({className})");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error processing teacher notification for class");
                                    }
                                }

                                await unitOfWork.SaveChangeAsync();
                                _logger.LogInformation("Teacher notifications sent for classes on their last day or recently ended");
                            }
                            else
                            {
                                _logger.LogInformation("No classes were processed for feedback forms");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No feedback forms created: {Message}", result.Message);
                        }
                    }

                    await Task.Delay(TimeSpan.FromHours(checkIntervalHours), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred while checking for classes requiring feedback");
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
            }
        }*/

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Class Feedback Background Service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running class feedback check at: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var feedbackNotificationService = scope.ServiceProvider.GetRequiredService<IFeedbackNotificationService>();
                        var result = await feedbackNotificationService.CheckForClassLastDayFeedbacksAsync();

                        if (result.IsSucceed && result.Data != null)
                        {
                            _logger.LogInformation("Created feedback forms for classes on their last day or recently ended: {Message}", result.Message);

                            if (result.Data is System.Collections.Generic.List<object> classesProcessed && classesProcessed.Count > 0)
                            {
                                var unitOfWork = scope.ServiceProvider.GetRequiredService<InstruLearn_Application.DAL.UoW.IUoW.IUnitOfWork>();

                                foreach (dynamic classInfo in classesProcessed)
                                {
                                    try
                                    {
                                        int classId = classInfo.ClassId;
                                        string className = classInfo.ClassName;
                                        int teacherId = classInfo.TeacherId;
                                        string teacherName = classInfo.TeacherName;
                                        int feedbacksCreated = classInfo.FeedbacksCreated;
                                        string levelName = classInfo.LevelName;
                                        string majorName = classInfo.MajorName;
                                        int templatesUsed = classInfo.TemplateId;
                                        string templateName = classInfo.TemplateName;
                                        bool isLastDay = classInfo.IsLastDay;
                                        bool isRecentlyEnded = classInfo.IsRecentlyEnded;
                                        DateOnly endDate = classInfo.EndDate;

                                        if (feedbacksCreated > 0)
                                        {
                                            string statusMessage = isLastDay
                                                ? "has reached its last day today"
                                                : $"ended on {endDate:dd/MM/yyyy} without complete feedback";

                                            var notification = new StaffNotification
                                            {
                                                Type = NotificationType.ClassFeedback,
                                                Status = NotificationStatus.Unread,
                                                CreatedAt = DateTime.Now,
                                                Title = $"Feedback Required for Class: {className}",
                                                Message = $"Please complete feedback forms for {feedbacksCreated} students in your {levelName} {majorName} class '{className}'. This class {statusMessage}. All feedback forms have been prepared with the template '{templateName}'."
                                            };

                                            await unitOfWork.StaffNotificationRepository.AddAsync(notification);
                                            _logger.LogInformation($"Created notification for teacher {teacherId} ({teacherName}) for class {classId} ({className}) that {statusMessage}");
                                        }
                                        else
                                        {
                                            _logger.LogInformation($"No new feedback forms were created for class {classId} ({className})");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error processing teacher notification for class");
                                    }
                                }

                                await unitOfWork.SaveChangeAsync();
                                _logger.LogInformation("Teacher notifications sent for classes on their last day or recently ended");
                            }
                            else
                            {
                                _logger.LogInformation("No classes were processed for feedback forms");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No feedback forms created: {Message}", result.Message);
                        }
                    }

                    // Wait for the next check interval (1 minute)
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred while checking for classes requiring feedback");
                    // Short delay on error before retrying
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }
    }
}