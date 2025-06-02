using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationDay;
using InstruLearn_Application.Model.Models.DTO.StaffNotification;
using Microsoft.EntityFrameworkCore;
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

                allNotifications = allNotifications?.Where(n => n.Status != NotificationStatus.Resolved).ToList();

                if (allNotifications == null || !allNotifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy yêu cầu thay đổi giáo viên nào.",
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
                        Message = "Không tìm thấy yêu cầu thay đổi giáo viên nào có trạng thái FourtyFeedbackDone.",
                        Data = new List<StaffNotificationDTO>()
                    };
                }

                var notificationDTOs = _mapper.Map<List<StaffNotificationDTO>>(notifications);

                for (int i = 0; i < notifications.Count; i++)
                {
                    if (notifications[i].LearningRegisId.HasValue)
                    {
                        var registrationData = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                            lr => lr.LearningRegisId == notifications[i].LearningRegisId.Value &&
                                  lr.Status == LearningRegis.FourtyFeedbackDone,
                            "Teacher,Learner");

                        var feedback = await _unitOfWork.LearningRegisFeedbackRepository
                            .GetFeedbackByRegistrationIdAsync(notifications[i].LearningRegisId.Value);

                        if (feedback != null)
                        {
                            notificationDTOs[i].TeacherChangeReason = feedback.TeacherChangeReason;

                            if (registrationData != null && registrationData.Any() && registrationData.First().Learner != null)
                            {
                                var learner = registrationData.First().Learner;
                                notificationDTOs[i].Message = $"Học viên {learner.FullName} muốn tiếp tục học nhưng thay đổi giáo viên.";
                            }
                            else
                            {
                                string message = notificationDTOs[i].Message;

                                int reasonIndex = message.IndexOf(".Lý do:");
                                if (reasonIndex > 0)
                                {
                                    notificationDTOs[i].Message = message.Substring(0, reasonIndex + 1);
                                }

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
                    Message = $"Đã truy xuất {notifications.Count} yêu cầu thay đổi giáo viên.",
                    Data = notificationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher change requests");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất yêu cầu thay đổi giáo viên: {ex.Message}"
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
                        Message = "Không tìm thấy yêu cầu thay đổi giáo viên nào.",
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
                        Message = "Không tìm thấy đăng ký học nào liên quan đến yêu cầu thay đổi giáo viên.",
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

                            var schedules = await _unitOfWork.ScheduleRepository
                                .GetSchedulesByLearningRegisIdAsync(regis.LearningRegisId);

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

                    var availableDayValues = new List<DayOfWeek>();

                    if (registration?.LearningRegistrationDay != null && registration.LearningRegistrationDay.Any())
                    {
                        foreach (var day in registration.LearningRegistrationDay)
                        {
                            string dayString = day.DayOfWeek.ToString();
                            dto.LearningDays.Add(dayString);

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

                    if (registration?.Schedules != null && registration.Schedules.Any())
                    {
                        _logger.LogInformation($"Processing schedules for registration ID: {registration.LearningRegisId}. Found {registration.Schedules.Count} total schedule(s)");

                        int totalSessions = registration.NumberOfSession;
                        int remainingSessions = (int)Math.Ceiling(totalSessions * 0.6);

                        _logger.LogInformation($"For registration {registration.LearningRegisId}: Total sessions: {totalSessions}, Remaining sessions (60%): {remainingSessions}");

                        var orderedSchedules = registration.Schedules
                            .OrderBy(s => s.StartDay)
                            .ThenBy(s => s.TimeStart)
                            .ToList();

                        int sessionsToSkip = totalSessions - remainingSessions;

                        if (sessionsToSkip < 0)
                            sessionsToSkip = 0;

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

                        int remainingSessions = (int)Math.Ceiling(registration.NumberOfSession * 0.6);

                        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);

                        var sessionDates = new List<string>();
                        int sessionsFound = 0;
                        int maxAttempts = 100;
                        int attempts = 0;

                        if (!availableDayValues.Contains(currentDate.DayOfWeek))
                        {
                            while (!availableDayValues.Contains(currentDate.DayOfWeek) && attempts < maxAttempts)
                            {
                                currentDate = currentDate.AddDays(1);
                                attempts++;
                            }
                        }

                        attempts = 0;
                        while (sessionsFound < remainingSessions && attempts < maxAttempts)
                        {
                            if (availableDayValues.Contains(currentDate.DayOfWeek))
                            {
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
                    Message = $"Đã truy xuất {registrationDTOs.Count} đăng ký học có yêu cầu thay đổi giáo viên.",
                    Data = registrationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher change request learning registrations");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất đăng ký học có yêu cầu thay đổi giáo viên: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetTeacherChangeRequestLearningRegistrationByIdAsync(int learningRegisId)
        {
            try
            {
                _logger.LogInformation($"Retrieving teacher change request learning registration with ID: {learningRegisId}");

                var notifications = await _unitOfWork.StaffNotificationRepository
                    .GetContinueWithTeacherChangeRequestsAsync();

                var matchingNotification = notifications?
                    .FirstOrDefault(n => n.LearningRegisId == learningRegisId);

                if (matchingNotification == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy yêu cầu thay đổi giáo viên nào cho ID đăng ký học: {learningRegisId}",
                        Data = null
                    };
                }

                var registration = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                    lr => lr.LearningRegisId == learningRegisId && lr.Status == LearningRegis.FourtyFeedbackDone,
                    "Teacher,Learner.Account,Major,Classes,LearningRegistrationDay,Learning_Registration_Type,LevelAssigned,Response.ResponseType");

                if (registration == null || !registration.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Đăng ký học với ID {learningRegisId} không tìm thấy hoặc không ở trạng thái FourtyFeedbackDone.",
                        Data = null
                    };
                }

                var regis = registration.First();

                if (regis.LearningRegistrationDay == null || !regis.LearningRegistrationDay.Any())
                {
                    var days = await _unitOfWork.LearningRegisDayRepository.GetWithIncludesAsync(
                        d => d.LearningRegisId == regis.LearningRegisId,
                        "");

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

                var schedules = await _unitOfWork.ScheduleRepository
                    .GetSchedulesByLearningRegisIdAsync(regis.LearningRegisId);

                regis.Schedules = schedules ?? new List<Schedules>();

                var dto = _mapper.Map<OneOnOneRegisDTO>(regis);

                if (regis.Response?.ResponseType != null)
                {
                    dto.ResponseTypeId = regis.Response.ResponseType.ResponseTypeId;
                    dto.ResponseTypeName = regis.Response.ResponseType.ResponseTypeName;
                }

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

                        if (Enum.TryParse<DayOfWeek>(dayString, true, out var dayOfWeek))
                        {
                            availableDayValues.Add(dayOfWeek);
                        }
                    }
                }

                dto.StartDay = regis.StartDay;
                dto.TimeStart = regis.TimeStart;
                dto.TimeLearning = regis.TimeLearning;
                dto.NumberOfSession = regis.NumberOfSession;
                dto.TimeEnd = regis.TimeStart.AddMinutes(regis.TimeLearning);

                if (regis.Schedules != null && regis.Schedules.Any())
                {
                    _logger.LogInformation($"Processing schedules for registration ID: {regis.LearningRegisId}. Found {regis.Schedules.Count} total schedule(s)");

                    int totalSessions = regis.NumberOfSession;
                    int remainingSessions = (int)Math.Ceiling(totalSessions * 0.6);

                    _logger.LogInformation($"For registration {regis.LearningRegisId}: Total sessions: {totalSessions}, Remaining sessions (60%): {remainingSessions}");

                    var orderedSchedules = regis.Schedules
                        .OrderBy(s => s.StartDay)
                        .ThenBy(s => s.TimeStart)
                        .ToList();

                    int sessionsToSkip = totalSessions - remainingSessions;

                    if (sessionsToSkip < 0)
                        sessionsToSkip = 0;

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

                    int remainingSessions = (int)Math.Ceiling(regis.NumberOfSession * 0.6);

                    DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);

                    var sessionDates = new List<string>();
                    int sessionsFound = 0;
                    int maxAttempts = 100;
                    int attempts = 0;

                    if (!availableDayValues.Contains(currentDate.DayOfWeek))
                    {
                        while (!availableDayValues.Contains(currentDate.DayOfWeek) && attempts < maxAttempts)
                        {
                            currentDate = currentDate.AddDays(1);
                            attempts++;
                        }
                    }

                    attempts = 0;
                    while (sessionsFound < remainingSessions && attempts < maxAttempts)
                    {
                        if (availableDayValues.Contains(currentDate.DayOfWeek))
                        {
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
                    Message = $"Đã truy xuất đăng ký học có yêu cầu thay đổi giáo viên với ID: {learningRegisId}",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving teacher change request learning registration with ID: {learningRegisId}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất đăng ký học có yêu cầu thay đổi giáo viên: {ex.Message}"
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
                    Message = "Thông báo đã được đánh dấu là đã đọc."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi đánh dấu thông báo là đã đọc: {ex.Message}"
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
                    Message = "Thông báo đã được đánh dấu là đã giải quyết."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as resolved", notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi đánh dấu thông báo là đã giải quyết: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> ChangeTeacherForLearningRegistrationAsync(int notificationId, int learningRegisId, int newTeacherId, string? changeReason)
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
                        Message = "Thông báo không hợp lệ hoặc không khớp với đăng ký học."
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
                        Message = "Không tìm thấy đăng ký học hoặc không ở trạng thái thích hợp."
                    };
                }

                var registration = learningRegis.First();

                var newTeacher = await _unitOfWork.TeacherRepository.GetByIdAsync(newTeacherId);
                if (newTeacher == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy giáo viên mới."
                    };
                }

                var originalTeacherId = registration.TeacherId;
                var originalTeacher = originalTeacherId.HasValue
                    ? await _unitOfWork.TeacherRepository.GetByIdAsync(originalTeacherId.Value)
                    : null;

                bool isSameTeacher = originalTeacherId.HasValue && originalTeacherId.Value == newTeacherId;
                _logger.LogInformation($"Teacher change request: Old teacher ID: {originalTeacherId}, New teacher ID: {newTeacherId}, Same teacher: {isSameTeacher}");

                if (isSameTeacher && string.IsNullOrWhiteSpace(changeReason))
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Phải cung cấp lý do khi quyết định giữ nguyên giáo viên."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    registration.TeacherId = newTeacherId;
                    registration.PaymentDeadline = DateTime.Now.AddDays(1);
                    registration.TeacherChangeProcessed = true;
                    registration.ChangeTeacherRequested = true;
                    registration.SentTeacherChangeReminder = false;
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(registration);

                    var schedules = await _unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegisId);
                    var futureSchedules = schedules.Where(s => s.StartDay >= DateOnly.FromDateTime(DateTime.Today) && s.AttendanceStatus == AttendanceStatus.NotYet).ToList();

                    foreach (var schedule in futureSchedules)
                    {
                        schedule.TeacherId = newTeacherId;
                        schedule.ChangeReason = isSameTeacher ?
                            changeReason :
                            (string.IsNullOrWhiteSpace(changeReason) ?
                                "Teacher change requested by learner" :
                                changeReason);
                        await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
                    }

                    notification.Status = NotificationStatus.Resolved;
                    await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                    _logger.LogInformation($"Directly marked notification {notificationId} as resolved");

                    var relatedNotifications = await _unitOfWork.StaffNotificationRepository
                        .GetQuery()
                        .Where(n => n.LearningRegisId == learningRegisId &&
                                   n.Type == NotificationType.TeacherChangeRequest &&
                                   n.NotificationId != notificationId)
                        .ToListAsync();

                    _logger.LogInformation($"Found {relatedNotifications.Count} teacher change notifications for learning registration {learningRegisId}");

                    foreach (var relatedNotification in relatedNotifications)
                    {
                        _logger.LogInformation($"Before update: Notification {relatedNotification.NotificationId} status: {relatedNotification.Status}");
                        relatedNotification.Status = NotificationStatus.Resolved;
                        await _unitOfWork.StaffNotificationRepository.UpdateAsync(relatedNotification);
                        _logger.LogInformation($"Marked notification {relatedNotification.NotificationId} as resolved");
                    }

                    await _unitOfWork.SaveChangeAsync();
                    await transaction.CommitAsync();

                    var verificationCheck = await _unitOfWork.StaffNotificationRepository
                        .GetQuery()
                        .Where(n => n.LearningRegisId == learningRegisId &&
                                n.Type == NotificationType.TeacherChangeRequest)
                        .ToListAsync();

                    foreach (var verifiedNotification in verificationCheck)
                    {
                        _logger.LogInformation($"Verification - Notification {verifiedNotification.NotificationId} status after transaction: {verifiedNotification.Status}");
                        if (verifiedNotification.Status != NotificationStatus.Resolved)
                        {
                            _logger.LogWarning($"Notification {verifiedNotification.NotificationId} was not properly marked as resolved!");
                        }
                    }

                    string effectiveReason = isSameTeacher ?
                        changeReason! :
                        (string.IsNullOrWhiteSpace(changeReason) ?
                            "Teacher change requested by learner" :
                            changeReason);

                    await SendTeacherChangeNotifications(registration, newTeacher, originalTeacher, changeReason, futureSchedules, isSameTeacher);

                    var attendedSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present || s.AttendanceStatus == AttendanceStatus.Absent);
                    var totalSessions = schedules.Count;
                    var completedPercentage = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = isSameTeacher
                            ? "Yêu cầu về giáo viên đã được xử lý. Giáo viên hiện tại sẽ tiếp tục dạy học viên này. Đã gửi thông báo."
                            : "Đã thay đổi giáo viên thành công cho đăng ký học và tất cả các lịch học trong tương lai. Đã gửi thông báo.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            NewTeacherId = newTeacherId,
                            NewTeacherName = newTeacher.Fullname,
                            OriginalTeacherId = originalTeacherId,
                            OriginalTeacherName = originalTeacher?.Fullname ?? "No previous teacher",
                            UpdatedSchedules = futureSchedules.Count,
                            CompletedSessions = attendedSessions,
                            CompletedPercentage = Math.Round(completedPercentage, 1),
                            RemainingSchedules = totalSessions - attendedSessions,
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

                var updatedNotification = await _unitOfWork.StaffNotificationRepository.GetByIdAsync(notificationId);
                if (updatedNotification != null && updatedNotification.Status != NotificationStatus.Resolved)
                {
                    _logger.LogWarning($"Notification {notificationId} still not marked as resolved after transaction. Attempting direct update.");

                    updatedNotification.Status = NotificationStatus.Resolved;
                    await _unitOfWork.StaffNotificationRepository.UpdateAsync(updatedNotification);
                    await _unitOfWork.SaveChangeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing teacher for learning registration {LearningRegisId}, notification {NotificationId}",
                    learningRegisId, notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể thay đổi giáo viên: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> GetTeacherNotificationsAsync(int teacherId)
        {
            try
            {
                _logger.LogInformation($"Retrieving notifications for teacher ID: {teacherId}");

                var notificationTypes = new[] {
                    NotificationType.CreateLearningPath,
                    NotificationType.SchedulesCreated,
                    NotificationType.ClassFeedback
                };

                List<StaffNotification> notifications = new List<StaffNotification>();

                try
                {
                    var directNotifications = await _unitOfWork.StaffNotificationRepository
                        .GetNotificationsByTeacherIdAsync(teacherId, notificationTypes);

                    if (directNotifications != null && directNotifications.Any())
                    {
                        notifications.AddRange(directNotifications);
                        _logger.LogInformation($"Found {directNotifications.Count} direct notifications for teacher {teacherId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting direct notifications for teacher {TeacherId}", teacherId);
                }

                try
                {
                    var learningRegistrations = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(lr => lr.TeacherId == teacherId, null);

                    if (learningRegistrations?.Any() == true)
                    {
                        var learningRegisIds = learningRegistrations.Select(lr => lr.LearningRegisId).ToList();
                        _logger.LogInformation($"Teacher {teacherId} has {learningRegisIds.Count} learning registrations");

                        if (learningRegisIds.Any())
                        {
                            var additionalNotifications = await _unitOfWork.StaffNotificationRepository
                                .GetWithIncludesAsync(n =>
                                    n.LearningRegisId.HasValue &&
                                    learningRegisIds.Contains(n.LearningRegisId.Value) &&
                                    n.Status != NotificationStatus.Resolved, "Learner");

                            var filteredAdditional = additionalNotifications?
                                .Where(n => notificationTypes.Contains(n.Type))
                                .ToList();

                            if (filteredAdditional?.Any() == true)
                            {
                                _logger.LogInformation($"Found {filteredAdditional.Count} additional notifications via learning registrations");

                                foreach (var notification in filteredAdditional)
                                {
                                    if (!notifications.Any(n => n.NotificationId == notification.NotificationId))
                                    {
                                        notifications.Add(notification);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting learning registration notifications for teacher {TeacherId}", teacherId);
                }

                try
                {
                    var messageBasedNotifications = await _unitOfWork.StaffNotificationRepository
                        .GetWithIncludesAsync(n =>
                            n.Type == NotificationType.ClassFeedback &&
                            (n.Message.Contains($"teacher {teacherId}") ||
                             n.Title.Contains($"Teacher {teacherId}")) &&
                            n.Status != NotificationStatus.Resolved, "Learner");

                    if (messageBasedNotifications?.Any() == true)
                    {
                        _logger.LogInformation($"Found {messageBasedNotifications.Count} message-based notifications for teacher {teacherId}");

                        foreach (var notification in messageBasedNotifications)
                        {
                            if (!notifications.Any(n => n.NotificationId == notification.NotificationId))
                            {
                                notifications.Add(notification);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting message-based notifications for teacher {TeacherId}", teacherId);
                }

                _logger.LogInformation($"Total notifications found: {notifications.Count} for teacher {teacherId}");

                if (notifications.Count == 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy thông báo nào cho giáo viên này.",
                        Data = new List<StaffNotificationDTO>()
                    };
                }

                try
                {
                    var learnerIds = notifications
                        .Where(n => n.LearnerId.HasValue && n.Learner == null)
                        .Select(n => n.LearnerId.Value)
                        .Distinct()
                        .ToList();

                    Dictionary<int, string> learnerNamesById = new Dictionary<int, string>();

                    if (learnerIds.Any())
                    {
                        var learners = await _unitOfWork.LearnerRepository
                            .GetWithIncludesAsync(l => learnerIds.Contains(l.LearnerId), null);

                        foreach (var learner in learners)
                        {
                            learnerNamesById[learner.LearnerId] = learner.FullName;
                        }
                    }

                    var learningRegisIds = notifications
                        .Where(n => n.LearningRegisId.HasValue && !n.LearnerId.HasValue)
                        .Select(n => n.LearningRegisId.Value)
                        .Distinct()
                        .ToList();

                    Dictionary<int, (int LearnerId, string LearnerName)> learnerByRegisId =
                        new Dictionary<int, (int LearnerId, string LearnerName)>();

                    if (learningRegisIds.Any())
                    {
                        var registrations = await _unitOfWork.LearningRegisRepository
                            .GetWithIncludesAsync(lr => learningRegisIds.Contains(lr.LearningRegisId), "Learner");

                        foreach (var reg in registrations)
                        {
                            if (reg.Learner != null)
                            {
                                learnerByRegisId[reg.LearningRegisId] = (reg.LearnerId, reg.Learner.FullName);
                            }
                        }
                    }

                    var notificationDTOs = new List<StaffNotificationDTO>();

                    foreach (var notification in notifications)
                    {
                        var dto = _mapper.Map<StaffNotificationDTO>(notification);

                        if (notification.Learner != null)
                        {
                            dto.LearnerName = notification.Learner.FullName;
                        }

                        else if (notification.LearnerId.HasValue && learnerNamesById.ContainsKey(notification.LearnerId.Value))
                        {
                            dto.LearnerName = learnerNamesById[notification.LearnerId.Value];
                        }

                        else if (notification.LearningRegisId.HasValue && learnerByRegisId.ContainsKey(notification.LearningRegisId.Value))
                        {
                            var (learnerId, learnerName) = learnerByRegisId[notification.LearningRegisId.Value];
                            dto.LearnerId = learnerId;
                            dto.LearnerName = learnerName;
                        }

                        else if (dto.LearnerName == "Unknown" && notification.Message != null)
                        {
                            if (notification.Message.Contains("viên "))
                            {
                                int startIndex = notification.Message.IndexOf("viên ") + 5;
                                int endIndex = notification.Message.IndexOf(" đã", startIndex);

                                if (endIndex > startIndex && endIndex - startIndex < 50)
                                {
                                    string extractedName = notification.Message.Substring(startIndex, endIndex - startIndex);
                                    dto.LearnerName = extractedName;
                                }
                            }
                        }

                        notificationDTOs.Add(dto);
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Đã truy xuất {notifications.Count} thông báo cho giáo viên có ID: {teacherId}",
                        Data = notificationDTOs
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error mapping notifications to DTOs for teacher {TeacherId}", teacherId);

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Retrieved {notifications.Count} notifications but encountered an error during processing: {ex.Message}",
                        Data = new List<object>()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notifications for teacher ID: {teacherId}");

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất thông báo: {ex.Message}",
                    Data = new List<object>()
                };
            }
        }


        private async Task SendTeacherChangeNotifications(
            Learning_Registration registration,
            Teacher newTeacher,
            Teacher originalTeacher,
            string? changeReason,
            List<Schedules> affectedSchedules,
            bool isSameTeacher = false)
        {
            string effectiveReason = isSameTeacher
                ? changeReason ?? "Teacher has been evaluated as the best fit for this learner"
                : string.IsNullOrWhiteSpace(changeReason)
                    ? "Teacher change requested by learner"
                    : changeReason;

            var nextSessionDate = affectedSchedules.Any()
                ? affectedSchedules.OrderBy(s => s.StartDay).First().StartDay.ToString("dd/MM/yyyy")
                : "upcoming sessions";

            if (registration.Learner?.Account != null && !string.IsNullOrEmpty(registration.Learner.Account.Email))
            {
                string learnerSubject = isSameTeacher ? "Teacher Request Processed" : "Teacher Change Notification";
                string learnerBody;

                decimal remainingPayment = registration.Price.HasValue ? registration.Price.Value * 0.6m : 0;
                string deadlineFormatted = registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                if (isSameTeacher)
                {
                    learnerBody = $@"
                    <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                    <h2 style='color: #333;'>Thông báo về yêu cầu thay đổi giáo viên</h2>
            
                    <p>Xin chào {registration.Learner.FullName},</p>
            
                    <p>Chúng tôi đã nhận được yêu cầu thay đổi giáo viên của bạn và đã xem xét tình huống.</p>
            
                    <div style='background-color: #e3f2fd; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #2196F3;'>
                        <h3 style='margin-top: 0; color: #333;'>Kết quả xem xét:</h3>
                        <p>Sau khi đánh giá, chúng tôi quyết định rằng giáo viên hiện tại của bạn <strong>{newTeacher.Fullname}</strong></p>
                        <p><strong>Lý do:</strong> {effectiveReason}</p>
                        <p><strong>Buổi học tiếp theo:</strong> {nextSessionDate}</p>
                    </div>
                    
                    <div style='background-color: #fff3cd; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                        <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
                        <p><strong>Số tiền cần thanh toán:</strong> {remainingPayment:N0} VND (60% học phí còn lại)</p>
                        <p><strong>Hạn thanh toán:</strong> {deadlineFormatted}</p>
                        <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
                        <p>Nếu không thanh toán trước hạn, đăng ký học của bạn sẽ bị hủy tự động.</p>
                    </div>
                    
                    <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                        <a href='https://instrulearn.com/payment/{registration.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                            Thanh Toán Ngay
                        </a>
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
                        {(!string.IsNullOrWhiteSpace(effectiveReason) ? $"<p><strong>Lý do thay đổi:</strong> {effectiveReason}</p>" : "")}
                    </div>
                    
                    <div style='background-color: #fff3cd; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                        <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
                        <p><strong>Số tiền cần thanh toán:</strong> {remainingPayment:N0} VND (60% học phí còn lại)</p>
                        <p><strong>Hạn thanh toán:</strong> {deadlineFormatted}</p>
                        <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
                        <p>Nếu không thanh toán trước hạn, đăng ký học của bạn sẽ bị hủy tự động.</p>
                    </div>
                    
                    <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                        <a href='https://instrulearn.com/payment/{registration.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                            Thanh Toán Ngay
                        </a>
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
