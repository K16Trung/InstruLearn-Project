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

        public ClassFeedbackBackgroundService(
            ILogger<ClassFeedbackBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Class Feedback Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running class last day feedback check job");

                try
                {
                    // Create a scope to resolve the required services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var feedbackService = scope.ServiceProvider.GetRequiredService<IFeedbackNotificationService>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<InstruLearn_Application.DAL.UoW.IUoW.IUnitOfWork>();

                        // Step 1: Create feedback forms for classes on their last day
                        var result = await feedbackService.CheckForClassLastDayFeedbacksAsync();

                        if (result.IsSucceed && result.Data != null)
                        {
                            _logger.LogInformation("Successfully created feedback forms for classes on their last day: {Message}", result.Message);

                            // Step 2: For each class that had feedback forms created, send notifications to teachers
                            if (result.Data is System.Collections.Generic.List<object> classesProcessed && classesProcessed.Count > 0)
                            {
                                foreach (dynamic classInfo in classesProcessed)
                                {
                                    try
                                    {
                                        // Extract class information
                                        int classId = classInfo.ClassId;
                                        string className = classInfo.ClassName;
                                        int teacherId = classInfo.TeacherId;
                                        string teacherName = classInfo.TeacherName;
                                        int feedbacksCreated = classInfo.FeedbacksCreated;
                                        string levelName = classInfo.LevelName;
                                        string majorName = classInfo.MajorName;
                                        int templatesUsed = classInfo.TemplateId;
                                        string templateName = classInfo.TemplateName;

                                        if (feedbacksCreated > 0)
                                        {
                                            // Send notification to teacher
                                            var notification = new StaffNotification
                                            {
                                                Type = NotificationType.ClassFeedback,
                                                Status = NotificationStatus.Unread,
                                                CreatedAt = DateTime.Now,
                                                Title = $"Feedback Required for Class: {className}",
                                                Message = $"Please complete feedback forms for {feedbacksCreated} students in your {levelName} {majorName} class '{className}'. This class has reached its last day. All feedback forms have been prepared with the template '{templateName}'."
                                            };

                                            await unitOfWork.StaffNotificationRepository.AddAsync(notification);
                                            _logger.LogInformation($"Created notification for teacher {teacherId} ({teacherName}) for class {classId} ({className})");
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
                                _logger.LogInformation("Teacher notifications sent for classes on their last day");
                            }
                            else
                            {
                                _logger.LogInformation("No classes were processed for feedback forms");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No feedback forms needed to be created: {Message}", result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing class feedback creation for last day classes");
                }

                // Run once per day (typically early morning)
                var nextRunTime = _configuration.GetValue<int>("ClassFeedback:CheckHour", 1);
                var now = DateTime.Now;
                var nextRun = now.Hour >= nextRunTime
                    ? now.Date.AddDays(1).AddHours(nextRunTime) // Run tomorrow
                    : now.Date.AddHours(nextRunTime); // Run today

                var delay = nextRun - now;

                _logger.LogInformation("Class Feedback Background Service is waiting until {NextRunTime}", nextRun);
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Class Feedback Background Service is stopping");
        }
    }
}
