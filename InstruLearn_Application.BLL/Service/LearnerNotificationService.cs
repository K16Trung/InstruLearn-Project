using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LearnerNotificationService : ILearnerNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StaffNotificationService> _logger;
        private readonly IEmailService _emailService;

        public LearnerNotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StaffNotificationService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }


        public async Task<ResponseDTO> GetLearnerEmailNotificationsAsync(int learnerId)
        {
            try
            {
                _logger.LogInformation($"Retrieving email notifications for learner ID: {learnerId}");

                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var learnerAccount = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                if (learnerAccount == null || string.IsNullOrEmpty(learnerAccount.Email))
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Học viên không có tài khoản email liên kết."
                    };
                }

                var emailNotifications = new List<NotificationDTO>();

                // Get staff notifications for this learner but filter out those that aren't meant for learners
                var allStaffNotifications = await _unitOfWork.StaffNotificationRepository.GetNotificationsByLearnerIdAsync(learnerId);

                // Define notification types that are relevant for learners
                var learnerRelevantNotificationTypes = new[]
                {
                    NotificationType.PaymentReminder,
                    NotificationType.FeedbackRequired,
                    NotificationType.Certificate,
                    NotificationType.CreateLearningPath,
                    NotificationType.RegistrationRejected
                };

                // Filter notifications to include only types relevant for learners
                var relevantStaffNotifications = allStaffNotifications
                    .Where(n =>
                        learnerRelevantNotificationTypes.Contains(n.Type) &&
                        !(n.Type == NotificationType.CreateLearningPath &&
                        n.Message.Contains("Vui lòng tạo lộ trình")))
                    .ToList();

                _logger.LogInformation($"Found {allStaffNotifications.Count} total staff notifications, filtered to {relevantStaffNotifications.Count} relevant notifications for learner ID: {learnerId}");

                foreach (var notification in relevantStaffNotifications)
                {
                    emailNotifications.Add(new NotificationDTO
                    {
                        Title = notification.Title,
                        Message = notification.Message,
                        RecipientEmail = learnerAccount.Email,
                        SentDate = notification.CreatedAt,
                        NotificationType = notification.Type,
                        Status = notification.Status,
                        LearningRegisId = notification.LearningRegisId
                    });
                }

                // Rest of the code remains the same
                var allLearningRegistrations = await _unitOfWork.LearningRegisRepository.GetRegistrationsByLearnerIdAsync(learnerId);
                var oneOnOneLearningRegistrations = allLearningRegistrations.Where(r => r.RegisTypeId == 1).ToList();
                _logger.LogInformation($"Found {oneOnOneLearningRegistrations.Count} 1:1 learning registrations for learner ID: {learnerId}");

                foreach (var registration in oneOnOneLearningRegistrations)
                {
                    // Payment deadline notifications
                    if (registration.Status == LearningRegis.FourtyFeedbackDone && registration.PaymentDeadline.HasValue)
                    {
                        decimal remainingPayment = registration.Price.HasValue ? registration.Price.Value * 0.6m : 0;

                        emailNotifications.Add(new NotificationDTO
                        {
                            Title = "Thanh toán học phí còn lại",
                            Message = $"Bạn cần thanh toán 60% học phí còn lại ({remainingPayment:N0} VND) trước hạn chót.",
                            RecipientEmail = learnerAccount.Email,
                            SentDate = DateTime.Now,
                            NotificationType = NotificationType.PaymentReminder,
                            Status = NotificationStatus.Unread,
                            LearningRegisId = registration.LearningRegisId,
                            Amount = remainingPayment,
                            Deadline = registration.PaymentDeadline,
                            LearningRequest = registration.LearningRequest
                        });
                    }

                    // Registration status change notifications
                    string statusMessage = "";
                    string statusTitle = "";

                    switch (registration.Status)
                    {
                        case LearningRegis.Accepted:
                            statusTitle = "Đăng ký học đã được chấp nhận";
                            statusMessage = "Đăng ký học của bạn đã được chấp nhận. Giáo viên sẽ chuẩn bị lộ trình học tập cho bạn.";
                            break;
                        case LearningRegis.Fourty:
                            statusTitle = "Thanh toán 40% học phí thành công";
                            statusMessage = "Bạn đã thanh toán 40% học phí. Bạn có thể bắt đầu học ngay bây giờ.";
                            break;
                        case LearningRegis.FourtyFeedbackDone:
                            statusTitle = "Cần thanh toán 60% học phí còn lại";
                            statusMessage = $"Cảm ơn bạn đã hoàn thành phản hồi. Vui lòng thanh toán 60% học phí còn lại trước {registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm")}";
                            break;
                        case LearningRegis.Sixty:
                            statusTitle = "Thanh toán 100% học phí thành công";
                            statusMessage = "Bạn đã thanh toán đủ 100% học phí. Bạn có thể tiếp tục học tập.";
                            break;
                        case LearningRegis.Rejected:
                            var rejectionNotification = relevantStaffNotifications
                                .FirstOrDefault(n => n.LearningRegisId == registration.LearningRegisId &&
                                                n.Type == NotificationType.RegistrationRejected);

                            if (rejectionNotification != null)
                            {
                                continue;
                            }

                            string rejectionReason = "Không có lý do cụ thể.";
                            if (registration.ResponseId.HasValue)
                            {
                                var response = await _unitOfWork.ResponseRepository.GetWithIncludesAsync(
                                    r => r.ResponseId == registration.ResponseId.Value, "ResponseType");

                                if (response != null && response.Any())
                                {
                                    var selectedResponse = response.First();
                                    rejectionReason = selectedResponse.ResponseName ?? rejectionReason;

                                    if (selectedResponse.ResponseType != null)
                                    {
                                        rejectionReason = $"{selectedResponse.ResponseType.ResponseTypeName}: {rejectionReason}";
                                    }
                                }
                            }
                            statusTitle = "Đăng ký học bị từ chối";
                            statusMessage = "Đăng ký học của bạn đã bị từ chối.";
                            break;
                        case LearningRegis.Cancelled:
                            statusTitle = "Đăng ký học đã bị hủy";
                            statusMessage = "Đăng ký học của bạn đã bị hủy.";
                            break;
                    }

                    if (!string.IsNullOrEmpty(statusMessage))
                    {
                        var existingStatusNotification = allStaffNotifications
                            .FirstOrDefault(n => n.LearningRegisId == registration.LearningRegisId &&
                                             n.Type == NotificationType.SchedulesCreated);
                        var notification = new NotificationDTO
                        {
                            Title = statusTitle,
                            Message = statusMessage,
                            RecipientEmail = learnerAccount.Email,
                            SentDate = DateTime.Now,
                            NotificationType = NotificationType.SchedulesCreated,
                            Status = existingStatusNotification?.Status ?? NotificationStatus.Unread,
                            LearningRegisId = registration.LearningRegisId,
                            LearningRequest = registration.LearningRequest
                        };

                        if (registration.Status == LearningRegis.Cancelled)
                        {
                            notification.Reason = "Quá hạn thanh toán 60%";
                        }
                        else if (registration.Status == LearningRegis.Rejected)
                        {
                            notification.Reason = "Quá hạn thanh toán 40%";
                        }

                        emailNotifications.Add(notification);
                    }

                    // Learning path confirmation notifications
                    if (registration.Status == LearningRegis.Accepted || registration.Status == LearningRegis.Fourty)
                    {
                        try
                        {
                            _logger.LogInformation($"Processing notification for registration ID: {registration.LearningRegisId}, " +
                                                  $"Status: {registration.Status}, HasPendingLearningPath: {registration.HasPendingLearningPath}");

                            // Check if learning paths actually exist
                            var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                                .GetByLearningRegisIdAsync(registration.LearningRegisId);

                            bool hasConfirmedLearningPath = learningPathSessions != null &&
                                                           learningPathSessions.Any() &&
                                                           registration.HasPendingLearningPath == false;

                            if (hasConfirmedLearningPath)
                            {
                                // Check if there's already a notification for this learning registration
                                var existingNotification = allStaffNotifications
                                    .FirstOrDefault(n => n.LearningRegisId == registration.LearningRegisId &&
                                                     n.Type == NotificationType.CreateLearningPath);

                                // Learning path exists and has been confirmed - show payment notification
                                int sessionCount = learningPathSessions.Count;
                                _logger.LogInformation($"Found {sessionCount} confirmed learning path session(s)");

                                decimal paymentAmount = registration.Price.HasValue ? registration.Price.Value * 0.4m : 0;

                                string teacherName = "Giáo viên";
                                if (registration.TeacherId.HasValue)
                                {
                                    var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(registration.TeacherId.Value);
                                    if (teacher != null)
                                    {
                                        teacherName = teacher.Fullname;
                                    }
                                }

                                string deadlineText = registration.PaymentDeadline.HasValue
                                    ? registration.PaymentDeadline.Value.ToString("dd/MM/yyyy HH:mm")
                                    : "";

                                emailNotifications.Add(new NotificationDTO
                                {
                                    Title = "Lộ trình học tập của bạn đã sẵn sàng",
                                    Message = $"{teacherName} đã chuẩn bị lộ trình học tập gồm {sessionCount} buổi học cho bạn. " +
                                              $"Vui lòng thanh toán 40% học phí ({paymentAmount:N0} VND) " +
                                              (string.IsNullOrEmpty(deadlineText) ? "" : $"trước {deadlineText} ") +
                                              "để bắt đầu học.",
                                    RecipientEmail = learnerAccount.Email,
                                    SentDate = DateTime.Now,
                                    NotificationType = NotificationType.CreateLearningPath,
                                    Status = existingNotification?.Status ?? NotificationStatus.Unread,
                                    LearningRegisId = registration.LearningRegisId,
                                    Amount = paymentAmount,
                                    Deadline = registration.PaymentDeadline,
                                    LearningRequest = registration.LearningRequest
                                });
                            }
                            // The "registration accepted" notification is already handled in the earlier switch statement
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing learning path notification for registration ID: {registration.LearningRegisId}");
                        }
                    }
                }

                // Feedback notifications
                var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByLearnerIdAsync(learnerId);
                _logger.LogInformation($"Found {feedbacks?.Count ?? 0} feedback notifications for learner ID: {learnerId}");

                foreach (var feedback in feedbacks)
                {
                    if (feedback.Status != FeedbackStatus.Completed)
                    {
                        emailNotifications.Add(new NotificationDTO
                        {
                            Title = "Yêu cầu đánh giá: Tiếp tục quá trình học tập",
                            Message = "Bạn đã đạt đến 40% hành trình học tập của mình và chúng tôi muốn nhận phản hồi của bạn trước khi bạn tiếp tục.",
                            RecipientEmail = learnerAccount.Email,
                            SentDate = feedback.CreatedAt,
                            NotificationType = NotificationType.FeedbackRequired,
                            Status = NotificationStatus.Unread,
                            LearningRegisId = feedback.LearningRegistrationId,
                        });
                    }
                }

                var sortedNotifications = emailNotifications.OrderByDescending(n => n.SentDate).ToList();
                _logger.LogInformation($"Returning {sortedNotifications.Count} total notifications for learner ID: {learnerId}");

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã lấy {sortedNotifications.Count} thông báo email cho học viên.",
                    Data = sortedNotifications
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email notifications for learner {LearnerId}", learnerId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy thông báo email: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetEntranceTestNotificationsAsync(int learnerId, int? classId = null)
        {
            try
            {
                _logger.LogInformation($"Retrieving entrance test notifications for learner ID: {learnerId}" +
                                      (classId.HasValue ? $", class ID: {classId}" : ""));

                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var query = _unitOfWork.StaffNotificationRepository.GetQuery()
                    .Where(n => n.LearnerId == learnerId &&
                                n.Type == NotificationType.EntranceTest);

                if (classId.HasValue)
                {
                    // If a specific class ID is provided, filter notifications for that class
                    var classInfo = await _unitOfWork.ClassRepository.GetByIdAsync(classId.Value);
                    if (classInfo == null)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Không tìm thấy lớp với ID {classId.Value}."
                        };
                    }

                    query = query.Where(n => n.Title.Contains(classInfo.ClassName));
                }

                var notifications = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

                if (notifications == null || !notifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy thông báo kiểm tra đầu vào cho học viên này.",
                        Data = new List<NotificationDTO>()
                    };
                }

                var learnerAccount = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                string email = learnerAccount != null ? learnerAccount.Email : "Không có email";

                var notificationDtos = notifications.Select(n => new NotificationDTO
                {
                    Title = n.Title,
                    Message = n.Message,
                    RecipientEmail = email,
                    SentDate = n.CreatedAt,
                    NotificationType = n.Type,
                    Status = n.Status,
                }).ToList();

                _logger.LogInformation($"Found {notificationDtos.Count} entrance test notifications for learner ID: {learnerId}");

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã lấy {notificationDtos.Count} thông báo kiểm tra đầu vào.",
                    Data = notificationDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving entrance test notifications for learner {learnerId}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy thông báo kiểm tra đầu vào: {ex.Message}"
                };
            }
        }
    }
}
