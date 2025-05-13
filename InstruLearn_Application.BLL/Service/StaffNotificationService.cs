using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationDay;
using InstruLearn_Application.Model.Models.DTO.StaffNotification;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class StaffNotificationService : IStaffNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StaffNotificationService> _logger;
        private readonly IEmailService _emailService;

        public StaffNotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StaffNotificationService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> GetAllTeacherChangeRequestsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all teacher change requests");

                var allNotifications = await _unitOfWork.StaffNotificationRepository.GetContinueWithTeacherChangeRequestsAsync();

                if (allNotifications == null || !allNotifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No teacher change requests found.",
                        Data = new List<StaffNotificationDTO>()
                    };
                }

                var learningRegisIds = allNotifications
                    .Where(n => n.LearningRegisId.HasValue)
                    .Select(n => n.LearningRegisId.Value)
                    .Distinct()
                    .ToList();

                var notifications = new List<StaffNotification>();

                foreach (var notification in allNotifications)
                {
                    if (notification.LearningRegisId.HasValue)
                    {
                        var registration = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                            lr => lr.LearningRegisId == notification.LearningRegisId.Value &&
                                  lr.Status == LearningRegis.FourtyFeedbackDone,
                            "Teacher,Learner");

                        if (registration != null && registration.Any())
                        {
                            notifications.Add(notification);
                        }
                    }
                }

                if (!notifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No teacher change requests found with FourtyFeedbackDone status.",
                        Data = new List<StaffNotificationDTO>()
                    };
                }

                var notificationDTOs = _mapper.Map<List<StaffNotificationDTO>>(notifications);

                for (int i = 0; i < notifications.Count; i++)
                {
                    if (notifications[i].LearningRegisId.HasValue)
                    {
                        // Get the registration data for this notification
                        var registrationData = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                            lr => lr.LearningRegisId == notifications[i].LearningRegisId.Value &&
                                  lr.Status == LearningRegis.FourtyFeedbackDone,
                            "Teacher,Learner");

                        // Retrieve the feedback to get the teacher change reason
                        var feedback = await _unitOfWork.LearningRegisFeedbackRepository
                            .GetFeedbackByRegistrationIdAsync(notifications[i].LearningRegisId.Value);

                        if (feedback != null)
                        {
                            notificationDTOs[i].TeacherChangeReason = feedback.TeacherChangeReason;

                            // Clean up the message by removing IDs and "Lý do:" prefix
                            if (registrationData != null && registrationData.Any() && registrationData.First().Learner != null)
                            {
                                var learner = registrationData.First().Learner;
                                notificationDTOs[i].Message = $"Học viên {learner.FullName} muốn tiếp tục học nhưng thay đổi giáo viên.";
                            }
                            else
                            {
                                // If we can't get the learner information, simply clean up the message
                                string message = notificationDTOs[i].Message;

                                // Find the index of "Lý do:" and remove it and everything after
                                int reasonIndex = message.IndexOf(".Lý do:");
                                if (reasonIndex > 0)
                                {
                                    notificationDTOs[i].Message = message.Substring(0, reasonIndex + 1);
                                }

                                // Remove the ID mentions
                                notificationDTOs[i].Message = System.Text.RegularExpressions.Regex.Replace(
                                    notificationDTOs[i].Message,
                                    @"\(ID: \d+\)",
                                    "");

                                notificationDTOs[i].Message = System.Text.RegularExpressions.Regex.Replace(
                                    notificationDTOs[i].Message,
                                    @"cho đăng ký học ID: \d+",
                                    "");
                            }
                        }
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {notifications.Count} teacher change requests.",
                    Data = notificationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher change requests");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving teacher change requests: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetTeacherChangeRequestLearningRegistrationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving learning registrations with teacher change requests");

                var notifications = await _unitOfWork.StaffNotificationRepository
                    .GetContinueWithTeacherChangeRequestsAsync();

                if (notifications == null || !notifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No teacher change requests found.",
                        Data = new List<object>()
                    };
                }

                var learningRegisIds = notifications
                    .Where(n => n.LearningRegisId.HasValue)
                    .Select(n => n.LearningRegisId.Value)
                    .Distinct()
                    .ToList();

                if (learningRegisIds.Count == 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No learning registrations associated with teacher change requests.",
                        Data = new List<object>()
                    };
                }

                var learningRegistrations = new List<Learning_Registration>();

                foreach (var id in learningRegisIds)
                {
                    var registration = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                        lr => lr.LearningRegisId == id && lr.Status == LearningRegis.FourtyFeedbackDone,
                        "Teacher,Learner.Account,Major,Classes,LearningRegistrationDay,Learning_Registration_Type,LevelAssigned,Response.ResponseType");

                    if (registration != null && registration.Any())
                    {
                        foreach (var regis in registration)
                        {
                            if (regis.LearningRegistrationDay == null || !regis.LearningRegistrationDay.Any())
                            {
                                var days = await _unitOfWork.LearningRegisDayRepository.GetWithIncludesAsync(
                                    d => d.LearningRegisId == regis.LearningRegisId,
                                    "");

                                // Ensure the days collection is initialized
                                if (regis.LearningRegistrationDay == null)
                                    regis.LearningRegistrationDay = new List<LearningRegistrationDay>();

                                if (days != null && days.Any())
                                {
                                    foreach (var day in days)
                                    {
                                        if (!regis.LearningRegistrationDay.Any(d => d.LearnRegisDayId == day.LearnRegisDayId))
                                        {
                                            regis.LearningRegistrationDay.Add(day);
                                        }
                                    }
                                }
                            }

                            // Get all schedules for this registration WITHOUT filtering by date
                            var schedules = await _unitOfWork.ScheduleRepository
                                .GetSchedulesByLearningRegisIdAsync(regis.LearningRegisId);

                            // Assign ALL schedules to the registration
                            regis.Schedules = schedules ?? new List<Schedules>();
                        }

                        learningRegistrations.AddRange(registration);
                    }
                }

                var registrationDTOs = _mapper.Map<List<OneOnOneRegisDTO>>(learningRegistrations);

                foreach (var dto in registrationDTOs)
                {
                    var registration = learningRegistrations.FirstOrDefault(lr => lr.LearningRegisId == dto.LearningRegisId);

                    if (registration?.Response?.ResponseType != null)
                    {
                        dto.ResponseTypeId = registration.Response.ResponseType.ResponseTypeId;
                        dto.ResponseTypeName = registration.Response.ResponseType.ResponseTypeName;
                    }

                    if (dto.LearningDays == null)
                        dto.LearningDays = new List<string>();

                    dto.LearningDays.Clear();

                    // Extract learning days and convert to DayOfWeek values
                    var availableDayValues = new List<DayOfWeek>();

                    if (registration?.LearningRegistrationDay != null && registration.LearningRegistrationDay.Any())
                    {
                        foreach (var day in registration.LearningRegistrationDay)
                        {
                            string dayString = day.DayOfWeek.ToString();
                            dto.LearningDays.Add(dayString);

                            // Convert the enum value to a DayOfWeek
                            if (Enum.TryParse<DayOfWeek>(dayString, true, out var dayOfWeek))
                            {
                                availableDayValues.Add(dayOfWeek);
                            }
                        }
                    }

                    dto.StartDay = registration?.StartDay;
                    dto.TimeStart = registration?.TimeStart ?? default;
                    dto.TimeLearning = registration?.TimeLearning ?? 0;
                    dto.NumberOfSession = registration?.NumberOfSession ?? 0;

                    if (registration != null)
                    {
                        dto.TimeEnd = registration.TimeStart.AddMinutes(registration.TimeLearning);
                    }

                    // Use ALL schedules from the database, but only include the remaining 60% of sessions (since it's FourtyFeedbackDone status)
                    if (registration?.Schedules != null && registration.Schedules.Any())
                    {
                        _logger.LogInformation($"Processing schedules for registration ID: {registration.LearningRegisId}. Found {registration.Schedules.Count} total schedule(s)");

                        // Calculate how many sessions should be included (60% of total since we're in FourtyFeedbackDone status)
                        int totalSessions = registration.NumberOfSession;
                        int remainingSessions = (int)Math.Ceiling(totalSessions * 0.6);

                        _logger.LogInformation($"For registration {registration.LearningRegisId}: Total sessions: {totalSessions}, Remaining sessions (60%): {remainingSessions}");

                        // Order by date and time
                        var orderedSchedules = registration.Schedules
                            .OrderBy(s => s.StartDay)
                            .ThenBy(s => s.TimeStart)
                            .ToList();

                        // Skip 40% and take 60% of sessions
                        // If we have 10 sessions total, we skip 4 (40%) and take 6 (60%)
                        int sessionsToSkip = totalSessions - remainingSessions;

                        // Make sure we don't skip more than we have
                        if (sessionsToSkip < 0)
                            sessionsToSkip = 0;

                        // Take only the remaining 60% of sessions
                        dto.SessionDates = orderedSchedules
                            .Skip(Math.Min(sessionsToSkip, orderedSchedules.Count))
                            .Take(remainingSessions)
                            .Select(s => $"{s.StartDay:yyyy-MM-dd} {s.TimeStart:HH:mm}")
                            .ToList();

                        _logger.LogInformation($"Remaining 60% session dates for registration {registration.LearningRegisId}: {string.Join(", ", dto.SessionDates)}");
                    }
                    else if (registration?.StartDay.HasValue == true &&
                             availableDayValues.Count > 0 &&
                             registration.NumberOfSession > 0)
                    {
                        _logger.LogWarning($"No schedules found for registration ID: {registration.LearningRegisId}. Using fallback date calculation.");

                        // Calculate remaining sessions (60% of total for FourtyFeedbackDone status)
                        int remainingSessions = (int)Math.Ceiling(registration.NumberOfSession * 0.6);

                        // Start from today
                        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);

                        var sessionDates = new List<string>();
                        int sessionsFound = 0;
                        int maxAttempts = 100; // Safety limit
                        int attempts = 0;

                        // Find the first valid day starting from today
                        if (!availableDayValues.Contains(currentDate.DayOfWeek))
                        {
                            while (!availableDayValues.Contains(currentDate.DayOfWeek) && attempts < maxAttempts)
                            {
                                currentDate = currentDate.AddDays(1);
                                attempts++;
                            }
                        }

                        attempts = 0;
                        // Generate dates for each remaining session
                        while (sessionsFound < remainingSessions && attempts < maxAttempts)
                        {
                            if (availableDayValues.Contains(currentDate.DayOfWeek))
                            {
                                // Format the date and add to the list
                                sessionDates.Add($"{currentDate:yyyy-MM-dd} {registration.TimeStart:HH:mm}");
                                sessionsFound++;
                            }

                            if (sessionsFound < remainingSessions)
                                currentDate = currentDate.AddDays(1);

                            attempts++;
                        }

                        dto.SessionDates = sessionDates;
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to calculate session dates for registration ID: {registration?.LearningRegisId}");
                        dto.SessionDates = new List<string>();
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {registrationDTOs.Count} learning registrations with teacher change requests.",
                    Data = registrationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher change request learning registrations");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving teacher change request learning registrations: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetTeacherChangeRequestLearningRegistrationByIdAsync(int learningRegisId)
        {
            try
            {
                _logger.LogInformation($"Retrieving teacher change request learning registration with ID: {learningRegisId}");

                // First, check if there is a notification with this learning registration ID
                var notifications = await _unitOfWork.StaffNotificationRepository
                    .GetContinueWithTeacherChangeRequestsAsync();

                var matchingNotification = notifications?
                    .FirstOrDefault(n => n.LearningRegisId == learningRegisId);

                if (matchingNotification == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"No teacher change request found for learning registration ID: {learningRegisId}",
                        Data = null
                    };
                }

                // Get the learning registration with the specific ID
                var registration = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                    lr => lr.LearningRegisId == learningRegisId && lr.Status == LearningRegis.FourtyFeedbackDone,
                    "Teacher,Learner.Account,Major,Classes,LearningRegistrationDay,Learning_Registration_Type,LevelAssigned,Response.ResponseType");

                if (registration == null || !registration.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Learning registration with ID {learningRegisId} not found or not in FourtyFeedbackDone status.",
                        Data = null
                    };
                }

                var regis = registration.First();

                // Load learning registration days if they're not already loaded
                if (regis.LearningRegistrationDay == null || !regis.LearningRegistrationDay.Any())
                {
                    var days = await _unitOfWork.LearningRegisDayRepository.GetWithIncludesAsync(
                        d => d.LearningRegisId == regis.LearningRegisId,
                        "");

                    // Ensure the days collection is initialized
                    if (regis.LearningRegistrationDay == null)
                        regis.LearningRegistrationDay = new List<LearningRegistrationDay>();

                    if (days != null && days.Any())
                    {
                        foreach (var day in days)
                        {
                            if (!regis.LearningRegistrationDay.Any(d => d.LearnRegisDayId == day.LearnRegisDayId))
                            {
                                regis.LearningRegistrationDay.Add(day);
                            }
                        }
                    }
                }

                // Get all schedules for this registration (without filtering by date)
                var schedules = await _unitOfWork.ScheduleRepository
                    .GetSchedulesByLearningRegisIdAsync(regis.LearningRegisId);

                // Assign schedules to the registration
                regis.Schedules = schedules ?? new List<Schedules>();

                // Map to DTO
                var dto = _mapper.Map<OneOnOneRegisDTO>(regis);

                // Set response type information
                if (regis.Response?.ResponseType != null)
                {
                    dto.ResponseTypeId = regis.Response.ResponseType.ResponseTypeId;
                    dto.ResponseTypeName = regis.Response.ResponseType.ResponseTypeName;
                }

                // Set learning days
                if (dto.LearningDays == null)
                    dto.LearningDays = new List<string>();
                else
                    dto.LearningDays.Clear();

                var availableDayValues = new List<DayOfWeek>();

                if (regis.LearningRegistrationDay != null && regis.LearningRegistrationDay.Any())
                {
                    foreach (var day in regis.LearningRegistrationDay)
                    {
                        string dayString = day.DayOfWeek.ToString();
                        dto.LearningDays.Add(dayString);

                        // Convert the enum value to a DayOfWeek
                        if (Enum.TryParse<DayOfWeek>(dayString, true, out var dayOfWeek))
                        {
                            availableDayValues.Add(dayOfWeek);
                        }
                    }
                }

                // Set basic properties
                dto.StartDay = regis.StartDay;
                dto.TimeStart = regis.TimeStart;
                dto.TimeLearning = regis.TimeLearning;
                dto.NumberOfSession = regis.NumberOfSession;
                dto.TimeEnd = regis.TimeStart.AddMinutes(regis.TimeLearning);

                // Handle session dates using the same logic as in GetTeacherChangeRequestLearningRegistrationsAsync
                if (regis.Schedules != null && regis.Schedules.Any())
                {
                    _logger.LogInformation($"Processing schedules for registration ID: {regis.LearningRegisId}. Found {regis.Schedules.Count} total schedule(s)");

                    // Calculate how many sessions should be included (60% of total since we're in FourtyFeedbackDone status)
                    int totalSessions = regis.NumberOfSession;
                    int remainingSessions = (int)Math.Ceiling(totalSessions * 0.6);

                    _logger.LogInformation($"For registration {regis.LearningRegisId}: Total sessions: {totalSessions}, Remaining sessions (60%): {remainingSessions}");

                    // Order by date and time
                    var orderedSchedules = regis.Schedules
                        .OrderBy(s => s.StartDay)
                        .ThenBy(s => s.TimeStart)
                        .ToList();

                    // Skip 40% and take 60% of sessions
                    int sessionsToSkip = totalSessions - remainingSessions;

                    // Make sure we don't skip more than we have
                    if (sessionsToSkip < 0)
                        sessionsToSkip = 0;

                    // Take only the remaining 60% of sessions
                    dto.SessionDates = orderedSchedules
                        .Skip(Math.Min(sessionsToSkip, orderedSchedules.Count))
                        .Take(remainingSessions)
                        .Select(s => $"{s.StartDay:yyyy-MM-dd} {s.TimeStart:HH:mm}")
                        .ToList();

                    _logger.LogInformation($"Remaining 60% session dates for registration {regis.LearningRegisId}: {string.Join(", ", dto.SessionDates)}");
                }
                else if (regis.StartDay.HasValue && availableDayValues.Count > 0 && regis.NumberOfSession > 0)
                {
                    _logger.LogWarning($"No schedules found for registration ID: {regis.LearningRegisId}. Using fallback date calculation.");

                    // Calculate remaining sessions (60% of total for FourtyFeedbackDone status)
                    int remainingSessions = (int)Math.Ceiling(regis.NumberOfSession * 0.6);

                    // Start from today
                    DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);

                    var sessionDates = new List<string>();
                    int sessionsFound = 0;
                    int maxAttempts = 100; // Safety limit
                    int attempts = 0;

                    // Find the first valid day starting from today
                    if (!availableDayValues.Contains(currentDate.DayOfWeek))
                    {
                        while (!availableDayValues.Contains(currentDate.DayOfWeek) && attempts < maxAttempts)
                        {
                            currentDate = currentDate.AddDays(1);
                            attempts++;
                        }
                    }

                    attempts = 0;
                    // Generate dates for each remaining session
                    while (sessionsFound < remainingSessions && attempts < maxAttempts)
                    {
                        if (availableDayValues.Contains(currentDate.DayOfWeek))
                        {
                            // Format the date and add to the list
                            sessionDates.Add($"{currentDate:yyyy-MM-dd} {regis.TimeStart:HH:mm}");
                            sessionsFound++;
                        }

                        if (sessionsFound < remainingSessions)
                            currentDate = currentDate.AddDays(1);

                        attempts++;
                    }

                    dto.SessionDates = sessionDates;
                }
                else
                {
                    _logger.LogWarning($"Unable to calculate session dates for registration ID: {regis.LearningRegisId}");
                    dto.SessionDates = new List<string>();
                }
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved teacher change request learning registration with ID: {learningRegisId}",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving teacher change request learning registration with ID: {learningRegisId}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving teacher change request learning registration: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                await _unitOfWork.StaffNotificationRepository.MarkAsReadAsync(notificationId);
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Notification marked as read."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error marking notification as read: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> MarkNotificationAsResolvedAsync(int notificationId)
        {
            try
            {
                await _unitOfWork.StaffNotificationRepository.MarkAsResolvedAsync(notificationId);
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Notification marked as resolved."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as resolved", notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error marking notification as resolved: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> ChangeTeacherForLearningRegistrationAsync(int notificationId, int learningRegisId, int newTeacherId, string changeReason)
        {
            try
            {
                _logger.LogInformation($"Processing teacher change request for learning registration {learningRegisId}, notification {notificationId}");

                var notification = await _unitOfWork.StaffNotificationRepository.GetByIdAsync(notificationId);
                if (notification == null || notification.Type != NotificationType.TeacherChangeRequest ||
                    notification.LearningRegisId != learningRegisId)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Invalid notification or notification doesn't match the learning registration."
                    };
                }

                var learningRegis = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                    lr => lr.LearningRegisId == learningRegisId && lr.Status == LearningRegis.FourtyFeedbackDone,
                    "Teacher,Learner.Account");

                if (learningRegis == null || !learningRegis.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning registration not found or not in appropriate status."
                    };
                }

                var registration = learningRegis.First();

                var newTeacher = await _unitOfWork.TeacherRepository.GetByIdAsync(newTeacherId);
                if (newTeacher == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "New teacher not found."
                    };
                }

                var originalTeacherId = registration.TeacherId;
                var originalTeacher = originalTeacherId.HasValue
                    ? await _unitOfWork.TeacherRepository.GetByIdAsync(originalTeacherId.Value)
                    : null;

                bool isSameTeacher = originalTeacherId.HasValue && originalTeacherId.Value == newTeacherId;
                _logger.LogInformation($"Teacher change request: Old teacher ID: {originalTeacherId}, New teacher ID: {newTeacherId}, Same teacher: {isSameTeacher}");

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    registration.TeacherId = newTeacherId;
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(registration);

                    var schedules = await _unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegisId);
                    var futureSchedules = schedules.Where(s => s.StartDay >= DateOnly.FromDateTime(DateTime.Today)).ToList();

                    foreach (var schedule in futureSchedules)
                    {
                        schedule.TeacherId = newTeacherId;
                        schedule.ChangeReason = changeReason;
                        await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
                    }

                    await _unitOfWork.StaffNotificationRepository.MarkAsResolvedAsync(notificationId);

                    await _unitOfWork.SaveChangeAsync();
                    await transaction.CommitAsync();

                    // Pass the isSameTeacher flag to the notification method
                    await SendTeacherChangeNotifications(registration, newTeacher, originalTeacher, changeReason, futureSchedules, isSameTeacher);

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = isSameTeacher
                            ? "Teacher request processed. The same teacher will continue teaching this learner. Notifications sent."
                            : "Teacher changed successfully for the learning registration and all future schedules. Notifications sent.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            NewTeacherId = newTeacherId,
                            NewTeacherName = newTeacher.Fullname,
                            OriginalTeacherId = originalTeacherId,
                            OriginalTeacherName = originalTeacher?.Fullname ?? "No previous teacher",
                            UpdatedSchedules = futureSchedules.Count,
                            NotificationResolved = true,
                            IsSameTeacher = isSameTeacher
                        }
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during teacher change transaction for learning registration {LearningRegisId}", learningRegisId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing teacher for learning registration {LearningRegisId}, notification {NotificationId}",
                    learningRegisId, notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to change teacher: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> GetTeacherNotificationsAsync(int teacherId)
        {
            try
            {
                _logger.LogInformation($"Retrieving notifications for teacher ID: {teacherId}");

                // Specify the notification types we want to filter by
                var notificationTypes = new[] {
            NotificationType.CreateLearningPath,
            NotificationType.SchedulesCreated
        };

                // Get notifications for this teacher with the specified types
                var notifications = await _unitOfWork.StaffNotificationRepository.GetNotificationsByTeacherIdAsync(
                    teacherId, notificationTypes);

                if (notifications == null || !notifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No notifications found for this teacher.",
                        Data = new List<StaffNotificationDTO>()
                    };
                }

                // Map to DTOs
                var notificationDTOs = _mapper.Map<List<StaffNotificationDTO>>(notifications);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {notifications.Count} notifications for teacher ID: {teacherId}",
                    Data = notificationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notifications for teacher ID: {teacherId}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving notifications: {ex.Message}"
                };
            }
        }


        private async Task SendTeacherChangeNotifications(
            Learning_Registration registration,
            Teacher newTeacher,
            Teacher originalTeacher,
            string changeReason,
            List<Schedules> affectedSchedules,
            bool isSameTeacher = false)
        {
            var nextSessionDate = affectedSchedules.Any()
                ? affectedSchedules.OrderBy(s => s.StartDay).First().StartDay.ToString("dd/MM/yyyy")
                : "upcoming sessions";

            // 1. Notify the learner
            if (registration.Learner?.Account != null && !string.IsNullOrEmpty(registration.Learner.Account.Email))
            {
                string learnerSubject = isSameTeacher ? "Teacher Request Processed" : "Teacher Change Notification";
                string learnerBody;

                if (isSameTeacher)
                {
                    // Custom message when the same teacher continues
                    learnerBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                            <h2 style='color: #333;'>Thông báo về yêu cầu thay đổi giáo viên</h2>
                    
                            <p>Xin chào {registration.Learner.FullName},</p>
                    
                            <p>Chúng tôi đã nhận được yêu cầu thay đổi giáo viên của bạn và đã xem xét tình huống.</p>
                    
                            <div style='background-color: #e3f2fd; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #2196F3;'>
                                <h3 style='margin-top: 0; color: #333;'>Kết quả xem xét:</h3>
                                <p>Sau khi đánh giá, chúng tôi quyết định rằng giáo viên hiện tại của bạn <strong>{newTeacher.Fullname}</strong> vẫn là phù hợp nhất để tiếp tục dạy bạn.</p>
                                <p><strong>Lý do:</strong> {changeReason}</p>
                                <p><strong>Buổi học tiếp theo:</strong> {nextSessionDate}</p>
                            </div>
                    
                            <p>Chúng tôi hiểu rằng mỗi học viên có nhu cầu học tập khác nhau. Nếu bạn gặp khó khăn, vui lòng cung cấp thêm chi tiết để chúng tôi có thể hỗ trợ bạn tốt hơn.</p>
                            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi.</p>
                    
                            <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                        </div>
                    </body>
                    </html>";
                }
                else
                {
                    // Original notification for teacher change
                    learnerBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                            <h2 style='color: #333;'>Thông báo thay đổi giáo viên</h2>
                    
                            <p>Xin chào {registration.Learner.FullName},</p>
                    
                            <p>Chúng tôi muốn thông báo rằng yêu cầu thay đổi giáo viên của bạn đã được chấp nhận.</p>
                    
                            <div style='background-color: #fff3cd; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ffc107;'>
                                <h3 style='margin-top: 0; color: #333;'>Chi tiết thay đổi:</h3>
                                <p><strong>Giáo viên cũ:</strong> {originalTeacher?.Fullname ?? "Chưa có giáo viên"}</p>
                                <p><strong>Giáo viên mới:</strong> {newTeacher.Fullname}</p>
                                <p><strong>Buổi học tiếp theo:</strong> {nextSessionDate}</p>
                                <p><strong>Lý do thay đổi:</strong> {changeReason}</p>
                            </div>
                    
                            <p>Tất cả các buổi học sắp tới của bạn sẽ được thực hiện với giáo viên mới.</p>
                            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi.</p>
                    
                            <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                        </div>
                    </body>
                    </html>";
                }

                await _emailService.SendEmailAsync(registration.Learner.Account.Email, learnerSubject, learnerBody, true);
                _logger.LogInformation("Sent teacher change notification email to learner {LearnerId}", registration.LearnerId);
            }

            // 2. Notify the teacher (whether new or same)
            if (newTeacher.AccountId != null)
            {
                var teacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(newTeacher.AccountId);
                if (teacherAccount != null && !string.IsNullOrEmpty(teacherAccount.Email))
                {
                    var sessionsByDate = affectedSchedules
                        .OrderBy(s => s.StartDay)
                        .GroupBy(s => s.StartDay)
                        .Take(5)
                        .Select(g => new
                        {
                            Date = g.Key.ToString("dd/MM/yyyy"),
                            DayOfWeek = g.Key.DayOfWeek.ToString(),
                            Sessions = g.Select(s => new
                            {
                                TimeStart = s.TimeStart.ToString("HH:mm"),
                                TimeEnd = s.TimeEnd.ToString("HH:mm")
                            }).ToList()
                        }).ToList();

                    string sessionsList = string.Join("", sessionsByDate.Select(d => $@"
                        <div style='margin-bottom: 10px;'>
                            <strong>{d.Date} ({d.DayOfWeek})</strong>:
                            {string.Join(", ", d.Sessions.Select(s => $"{s.TimeStart}-{s.TimeEnd}"))}
                        </div>"));

                    if (sessionsList.Length == 0)
                    {
                        sessionsList = "<p>Chi tiết lịch học sẽ được cập nhật trong thời gian tới.</p>";
                    }

                    string teacherSubject = isSameTeacher
                        ? "Xác nhận tiếp tục giảng dạy"
                        : "Phân công lớp học mới";

                    string teacherBody;

                    if (isSameTeacher)
                    {
                        // Email for same teacher continuing
                        teacherBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                <h2 style='color: #333;'>Thông báo xác nhận tiếp tục giảng dạy</h2>
                        
                                <p>Xin chào {newTeacher.Fullname},</p>
                        
                                <p>Chúng tôi xin thông báo rằng sau khi xem xét yêu cầu thay đổi giáo viên từ học viên {registration.Learner?.FullName ?? "N/A"}, 
                           quyết định của chúng tôi là bạn sẽ tiếp tục làm giáo viên cho học viên này.</p>
                        
                                <div style='background-color: #e8f5e9; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
                                    <h3 style='margin-top: 0; color: #333;'>Chi tiết:</h3>
                                    <p><strong>Học viên:</strong> {registration.Learner?.FullName ?? "N/A"}</p>
                                    <p><strong>Môn học:</strong> {registration.Major?.MajorName ?? "N/A"}</p>
                                    <p><strong>Lý do quyết định:</strong> {changeReason}</p>
                                    <p><strong>Lịch học sắp tới:</strong></p>
                                    {sessionsList}
                                    {(affectedSchedules.Count > 5 ? "<p><em>...và các buổi học khác</em></p>" : "")}
                                </div>
                        
                                <p>Vui lòng tiếp tục cung cấp trải nghiệm học tập chất lượng cao cho học viên này.</p>
                                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với quản trị viên.</p>
                        
                                <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                            </div>
                        </body>
                        </html>";
                    }
                    else
                    {
                        // Original email for new teacher assignment
                        teacherBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                <h2 style='color: #333;'>Thông báo phân công giảng dạy mới</h2>
                        
                                <p>Xin chào {newTeacher.Fullname},</p>
                        
                                <p>Bạn đã được phân công giảng dạy cho học viên {registration.Learner?.FullName ?? "N/A"}.</p>
                        
                                <div style='background-color: #e8f5e9; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
                                    <h3 style='margin-top: 0; color: #333;'>Chi tiết phân công:</h3>
                                    <p><strong>Học viên:</strong> {registration.Learner?.FullName ?? "N/A"}</p>
                                    <p><strong>Môn học:</strong> {registration.Major?.MajorName ?? "N/A"}</p>
                                    <p><strong>Lý do được phân công:</strong> {changeReason}</p>
                                    <p><strong>Lịch học sắp tới:</strong></p>
                                    {sessionsList}
                                    {(affectedSchedules.Count > 5 ? "<p><em>...và các buổi học khác</em></p>" : "")}
                                </div>
                        
                                <p>Vui lòng kiểm tra hệ thống để biết thêm chi tiết về lịch giảng dạy của bạn.</p>
                                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với quản trị viên.</p>
                        
                                <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                            </div>
                        </body>
                        </html>";
                    }

                    await _emailService.SendEmailAsync(teacherAccount.Email, teacherSubject, teacherBody, true);
                    _logger.LogInformation("Sent notification email to {0} teacher {1}",
                        isSameTeacher ? "continuing" : "new", newTeacher.TeacherId);
                }
            }

            // 3. Notify the original teacher only if different from new teacher
            if (!isSameTeacher && originalTeacher != null && originalTeacher.AccountId != null)
            {
                var originalTeacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(originalTeacher.AccountId);
                if (originalTeacherAccount != null && !string.IsNullOrEmpty(originalTeacherAccount.Email))
                {
                    string originalTeacherSubject = "Thay đổi lớp giảng dạy";
                    string originalTeacherBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                <h2 style='color: #333;'>Thông báo thay đổi lớp giảng dạy</h2>
                        
                                <p>Xin chào {originalTeacher.Fullname},</p>
                        
                                <p>Chúng tôi muốn thông báo rằng bạn sẽ không còn giảng dạy cho học viên {registration.Learner?.FullName ?? "N/A"}.</p>
                        
                                <div style='background-color: #ffebee; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #f44336;'>
                                    <h3 style='margin-top: 0; color: #333;'>Chi tiết thay đổi:</h3>
                                    <p><strong>Học viên:</strong> {registration.Learner?.FullName ?? "N/A"}</p>
                                    <p><strong>Môn học:</strong> {registration.Major?.MajorName ?? "N/A"}</p>
                                    <p><strong>Giáo viên mới:</strong> {newTeacher.Fullname}</p>
                                    <p><strong>Lý do thay đổi:</strong> {changeReason}</p>
                                </div>
                        
                                <p>Lịch giảng dạy của bạn đã được cập nhật.</p>
                                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với quản trị viên.</p>
                        
                                <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                            </div>
                        </body>
                        </html>";

                    await _emailService.SendEmailAsync(originalTeacherAccount.Email, originalTeacherSubject, originalTeacherBody, true);
                    _logger.LogInformation("Sent notification email to original teacher {TeacherId}", originalTeacher.TeacherId);
                }
            }
        }


    }
}
