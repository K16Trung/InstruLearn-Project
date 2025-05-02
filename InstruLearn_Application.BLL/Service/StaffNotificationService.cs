using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
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

                // Get notifications with included relationships
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

                // Build a list of learning registration IDs
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

                // Get all learning registrations with their related entities
                var learningRegistrations = new List<Learning_Registration>();

                foreach (var id in learningRegisIds)
                {
                    // Include necessary related entities including nested navigation properties
                    var registration = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                        lr => lr.LearningRegisId == id && lr.Status == LearningRegis.FourtyFeedbackDone,
                        "Teacher,Learner.Account,Major,Classes,LearningRegistrationDay,Learning_Registration_Type,LevelAssigned,Response.ResponseType");

                    if (registration != null && registration.Any())
                    {
                        learningRegistrations.AddRange(registration);
                    }
                }

                var registrationDTOs = _mapper.Map<List<OneOnOneRegisDTO>>(learningRegistrations);

                // Manually set response type information if needed
                foreach (var dto in registrationDTOs)
                {
                    var registration = learningRegistrations.FirstOrDefault(lr => lr.LearningRegisId == dto.LearningRegisId);
                    if (registration?.Response?.ResponseType != null)
                    {
                        dto.ResponseTypeId = registration.Response.ResponseType.ResponseTypeId;
                        dto.ResponseTypeName = registration.Response.ResponseType.ResponseTypeName;
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

                    await SendTeacherChangeNotifications(registration, newTeacher, originalTeacher, changeReason, futureSchedules);

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Teacher changed successfully for the learning registration and all future schedules. Notifications sent.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            NewTeacherId = newTeacherId,
                            NewTeacherName = newTeacher.Fullname,
                            OriginalTeacherId = originalTeacherId,
                            OriginalTeacherName = originalTeacher?.Fullname ?? "No previous teacher",
                            UpdatedSchedules = futureSchedules.Count,
                            NotificationResolved = true
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
            List<Schedules> affectedSchedules)
        {
            var nextSessionDate = affectedSchedules.Any()
                ? affectedSchedules.OrderBy(s => s.StartDay).First().StartDay.ToString("dd/MM/yyyy")
                : "upcoming sessions";

            // 1. Notify the learner
            if (registration.Learner?.Account != null && !string.IsNullOrEmpty(registration.Learner.Account.Email))
            {
                string learnerSubject = "Teacher Change Notification";
                string learnerBody = $@"
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

                await _emailService.SendEmailAsync(registration.Learner.Account.Email, learnerSubject, learnerBody, true);
                _logger.LogInformation("Sent teacher change notification email to learner {LearnerId}", registration.LearnerId);
            }

            // 2. Notify the new teacher
            if (newTeacher.AccountId != null)
            {
                var newTeacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(newTeacher.AccountId);
                if (newTeacherAccount != null && !string.IsNullOrEmpty(newTeacherAccount.Email))
                {
                    var sessionsByDate = affectedSchedules
                        .OrderBy(s => s.StartDay)
                        .GroupBy(s => s.StartDay)
                        .Take(5)
                        .Select(g => new {
                            Date = g.Key.ToString("dd/MM/yyyy"),
                            DayOfWeek = g.Key.DayOfWeek.ToString(),
                            Sessions = g.Select(s => new {
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

                    string newTeacherSubject = "Phân công lớp học mới";
                    string newTeacherBody = $@"
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

                    await _emailService.SendEmailAsync(newTeacherAccount.Email, newTeacherSubject, newTeacherBody, true);
                    _logger.LogInformation("Sent assignment notification email to new teacher {TeacherId}", newTeacher.TeacherId);
                }
            }

            // 3. Notify the original teacher (if applicable)
            if (originalTeacher != null && originalTeacher.AccountId != null && originalTeacher.TeacherId != newTeacher.TeacherId)
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
