using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
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
    public class PaymentReminderService : BackgroundService, IPaymentReminderService
    {
        private readonly ILogger<PaymentReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public PaymentReminderService(
            ILogger<PaymentReminderService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ nhắc nhở thanh toán bắt đầu lúc: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPaymentRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Đã xảy ra lỗi khi xử lý nhắc nhở thanh toán");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessPaymentRemindersAsync()
        {
            _logger.LogInformation("Kiểm tra các khoản thanh toán đang chờ cần nhắc nhở lúc: {time}", DateTimeOffset.Now);

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var registrations = await unitOfWork.LearningRegisRepository
                .GetWithIncludesAsync(
                    x => (x.Status == LearningRegis.Accepted || x.Status == LearningRegis.FourtyFeedbackDone) &&
                         x.PaymentDeadline.HasValue,
                    "Learner,Learner.Account,Teacher"
                );

            if (registrations == null || !registrations.Any())
            {
                _logger.LogInformation("Không tìm thấy đăng ký học tập nào cần nhắc nhở thanh toán");
                return;
            }

            _logger.LogInformation("Tìm thấy {count} đăng ký học tập có thể cần nhắc nhở thanh toán", registrations.Count);

            int remindersSent = 0;
            DateTime now = DateTime.Now;

            foreach (var registration in registrations)
            {
                try
                {
                    if (!registration.PaymentDeadline.HasValue || !registration.LastReminderSent.HasValue)
                    {
                        registration.LastReminderSent = null;
                        registration.ReminderCount = 0;
                    }

                    bool shouldSendReminder = false;
                    string reminderType = "";
                    TimeSpan reminderInterval;
                    int maxReminders;

                    if (registration.Status == LearningRegis.Accepted)
                    {
                        reminderInterval = TimeSpan.FromDays(1);
                        maxReminders = 3;
                        reminderType = "LearningPath";
                    }
                    else if (registration.Status == LearningRegis.FourtyFeedbackDone)
                    {
                        reminderInterval = TimeSpan.FromHours(6);
                        maxReminders = 4;
                        reminderType = "Feedback";
                    }
                    else
                    {
                        continue;
                    }

                    if ((!registration.LastReminderSent.HasValue ||
                         now - registration.LastReminderSent.Value >= reminderInterval) &&
                        registration.ReminderCount < maxReminders &&
                        now < registration.PaymentDeadline.Value)
                    {
                        shouldSendReminder = true;
                    }

                    if (registration.Status == LearningRegis.FourtyFeedbackDone &&
                        registration.ChangeTeacherRequested &&
                        registration.TeacherChangeProcessed &&
                        !registration.SentTeacherChangeReminder)
                    {
                        shouldSendReminder = true;
                        reminderType = "TeacherChange";
                    }

                    if (shouldSendReminder)
                    {
                        bool success = await SendPaymentReminderEmailAsync(
                            emailService,
                            unitOfWork,
                            registration,
                            reminderType);

                        if (success)
                        {
                            registration.LastReminderSent = now;
                            registration.ReminderCount++;

                            if (reminderType == "TeacherChange")
                            {
                                registration.SentTeacherChangeReminder = true;
                            }

                            await unitOfWork.LearningRegisRepository.UpdateAsync(registration);
                            await unitOfWork.SaveChangeAsync();

                            remindersSent++;
                            _logger.LogInformation("Đã gửi nhắc nhở thanh toán {count} trong tổng số {max} cho đăng ký {id}, loại: {type}",
                                registration.ReminderCount, maxReminders, registration.LearningRegisId, reminderType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xử lý nhắc nhở thanh toán cho đăng ký {id}",
                        registration.LearningRegisId);
                }
            }

            _logger.LogInformation("Kiểm tra nhắc nhở thanh toán hoàn tất. Đã gửi {count} nhắc nhở", remindersSent);
        }

        private async Task<bool> SendPaymentReminderEmailAsync(
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    Learning_Registration registration,
    string reminderType)
        {
            if (registration.Learner?.Account?.Email == null)
            {
                _logger.LogWarning("Không thể gửi nhắc nhở đến học viên {id} - không tìm thấy địa chỉ email", registration.LearnerId);
                return false;
            }

            string learnerEmail = registration.Learner.Account.Email;
            string learnerName = registration.Learner.FullName;

            TimeSpan timeUntilDeadline = registration.PaymentDeadline.Value - DateTime.Now;
            int hoursRemaining = Math.Max(0, (int)timeUntilDeadline.TotalHours);
            int daysRemaining = Math.Max(0, (int)timeUntilDeadline.TotalDays);

            decimal amountDue = 0;
            string paymentPercentage = "";

            if (registration.Status == LearningRegis.Accepted)
            {
                amountDue = registration.Price.HasValue ? registration.Price.Value * 0.4m : 0;
                paymentPercentage = "40%";
            }
            else
            {
                amountDue = registration.Price.HasValue ? registration.Price.Value * 0.6m : 0;
                paymentPercentage = "60%";
            }

            string subject = "";
            string body = "";

            if (reminderType == "TeacherChange")
            {
                subject = "Giáo viên mới đã được chỉ định - Thanh toán để tiếp tục học";
                body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
        <h2 style='color: #333;'>Xin chào {learnerName},</h2>
        
        <p>Chúng tôi vui mừng thông báo rằng <strong>yêu cầu thay đổi giáo viên của bạn đã được xử lý thành công</strong>.</p>
        
        <p>Giáo viên mới của bạn là <strong>{registration.Teacher?.Fullname ?? "N/A"}</strong>. Lịch học của bạn đã được cập nhật.</p>
        
        <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
            <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
            <p><strong>Số tiền cần thanh toán ({paymentPercentage}):</strong> {amountDue:N0} VND</p>
            <p><strong>Hạn thanh toán:</strong> {registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm")}</p>
            <p><strong>Còn lại:</strong> {(hoursRemaining < 24 ? $"{hoursRemaining} giờ" : $"{daysRemaining} ngày")}</p>
            <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
        </div>
        
        <p>Vui lòng thanh toán số tiền còn lại trước hạn để tiếp tục học tập với giáo viên mới.</p>
        
        <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
            <a href='http://localhost:3000/payment/{registration.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                Thanh Toán Ngay
            </a>
        </div>
        
        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
        
        <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
    </div>
</body>
</html>";
            }
            else
            {
                int reminderNumber = registration.ReminderCount + 1;
                string urgencyLevel = "";

                if (reminderType == "LearningPath")
                {
                    if (reminderNumber == 1) urgencyLevel = "Nhắc nhở";
                    else if (reminderNumber == 2) urgencyLevel = "Nhắc nhở lần 2";
                    else urgencyLevel = "QUAN TRỌNG - Nhắc nhở cuối cùng";

                    subject = $"{urgencyLevel}: Thanh toán {paymentPercentage} học phí để bắt đầu khóa học";
                    body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
        <h2 style='color: #333;'>Xin chào {learnerName},</h2>
        
        <p>Đây là lời nhắc nhở về việc thanh toán {paymentPercentage} học phí ban đầu cho khóa học của bạn. Lộ trình học tập đã được giáo viên tạo và đang chờ bạn.</p>
        
        <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
            <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
            <p><strong>Số tiền cần thanh toán ({paymentPercentage}):</strong> {amountDue:N0} VND</p>
            <p><strong>Hạn thanh toán:</strong> {registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm")}</p>
            <p><strong>Còn lại:</strong> {(hoursRemaining < 24 ? $"{hoursRemaining} giờ" : $"{daysRemaining} ngày")}</p>
            <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
        </div>
        
        <p>Nếu không thanh toán trước hạn, đăng ký học của bạn sẽ bị hủy tự động.</p>
        
        <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
            <a href='http://localhost:3000/payment/{registration.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                Thanh Toán Ngay
            </a>
        </div>
        
        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
        
        <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
    </div>
</body>
</html>";
                }
                else
                {
                    if (reminderNumber == 1) urgencyLevel = "Nhắc nhở";
                    else if (reminderNumber == 2) urgencyLevel = "Nhắc nhở lần 2";
                    else if (reminderNumber == 3) urgencyLevel = "Nhắc nhở lần 3";
                    else urgencyLevel = "KHẨN CẤP - Nhắc nhở cuối cùng";

                    subject = $"{urgencyLevel}: Thanh toán {paymentPercentage} học phí còn lại";
                    body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
        <h2 style='color: #333;'>Xin chào {learnerName},</h2>
        
        <p>Đây là lời nhắc nhở về việc thanh toán {paymentPercentage} học phí còn lại cho khóa học của bạn sau khi bạn đã hoàn thành đánh giá.</p>
        
        <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
            <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
            <p><strong>Số tiền cần thanh toán ({paymentPercentage}):</strong> {amountDue:N0} VND</p>
            <p><strong>Hạn thanh toán:</strong> {registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm")}</p>
            <p><strong>Còn lại:</strong> {(hoursRemaining < 24 ? $"{hoursRemaining} giờ" : $"{daysRemaining} ngày")}</p>
            <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
        </div>
        
        <p>Nếu không thanh toán trước hạn, đăng ký học của bạn sẽ bị hủy tự động và lịch học đã đặt sẽ bị xóa.</p>
        
        <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
            <a href='http://localhost:3000/payment/{registration.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                Thanh Toán Ngay
            </a>
        </div>
        
        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
        
        <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
    </div>
</body>
</html>";
                }
            }

            try
            {
                await emailService.SendEmailAsync(
                    learnerEmail,
                    subject,
                    body,
                    isHtml: true
                );

                var notification = new StaffNotification
                {
                    Title = subject,
                    Message = $"Đã gửi email nhắc nhở thanh toán cho đăng ký học ID: {registration.LearningRegisId}. " +
                             $"Lần nhắc nhở: {registration.ReminderCount + 1}. Hạn thanh toán: {registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm")}",
                    LearningRegisId = registration.LearningRegisId,
                    LearnerId = registration.LearnerId,
                    CreatedAt = DateTime.Now,
                    Status = NotificationStatus.Unread,
                    Type = NotificationType.PaymentReminder
                };

                await unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await unitOfWork.SaveChangeAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể gửi email nhắc nhở thanh toán đến học viên {id} tại {email}",
                    registration.LearnerId, learnerEmail);
                return false;
            }
        }
        public async Task<ResponseDTO> SendManualPaymentReminderAsync(int learningRegisId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var registration = await unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearningRegisId == learningRegisId,
                        "Learner,Learner.Account,Teacher"
                    )
                    .ContinueWith(t => t.Result?.FirstOrDefault());

                if (registration == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy đăng ký học tập"
                    };
                }

                if (registration.Status != LearningRegis.Accepted &&
                    registration.Status != LearningRegis.FourtyFeedbackDone)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Trạng thái đăng ký học tập {registration.Status} không đủ điều kiện để gửi nhắc nhở thanh toán"
                    };
                }

                string reminderType = registration.Status == LearningRegis.Accepted ? "LearningPath" : "Feedback";

                bool success = await SendPaymentReminderEmailAsync(
                    emailService,
                    unitOfWork,
                    registration,
                    reminderType);

                if (success)
                {
                    registration.LastReminderSent = DateTime.Now;
                    registration.ReminderCount++;

                    await unitOfWork.LearningRegisRepository.UpdateAsync(registration);
                    await unitOfWork.SaveChangeAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã gửi nhắc nhở thanh toán thành công",
                        Data = new
                        {
                            LearningRegisId = registration.LearningRegisId,
                            LearnerId = registration.LearnerId,
                            LearnerName = registration.Learner?.FullName,
                            ReminderCount = registration.ReminderCount,
                            SentAt = DateTime.Now
                        }
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không thể gửi nhắc nhở thanh toán"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending manual payment reminder for registration {id}", learningRegisId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi gửi nhắc nhở thanh toán: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetPaymentReminderStatisticsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pendingRegistrations = await unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => (x.Status == LearningRegis.Accepted || x.Status == LearningRegis.FourtyFeedbackDone) &&
                             x.PaymentDeadline.HasValue,
                        "Learner"
                    );

                if (pendingRegistrations == null || !pendingRegistrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy đăng ký thanh toán đang chờ",
                        Data = new
                        {
                            TotalCount = 0,
                            RemindersSent = 0,
                            PendingRegistrations = new object[] { }
                        }
                    };
                }

                var stats = new
                {
                    TotalCount = pendingRegistrations.Count,
                    RemindersSent = pendingRegistrations.Sum(r => r.ReminderCount),
                    ByStatus = new
                    {
                        Accepted = pendingRegistrations.Count(r => r.Status == LearningRegis.Accepted),
                        FourtyFeedbackDone = pendingRegistrations.Count(r => r.Status == LearningRegis.FourtyFeedbackDone)
                    },
                    ByReminderCount = new
                    {
                        NoReminders = pendingRegistrations.Count(r => r.ReminderCount == 0),
                        OneReminder = pendingRegistrations.Count(r => r.ReminderCount == 1),
                        TwoReminders = pendingRegistrations.Count(r => r.ReminderCount == 2),
                        ThreeReminders = pendingRegistrations.Count(r => r.ReminderCount == 3),
                        FourOrMoreReminders = pendingRegistrations.Count(r => r.ReminderCount >= 4)
                    },
                    ByDeadline = new
                    {
                        PastDue = pendingRegistrations.Count(r => r.PaymentDeadline < DateTime.Now),
                        DueToday = pendingRegistrations.Count(r =>
                            r.PaymentDeadline >= DateTime.Today &&
                            r.PaymentDeadline < DateTime.Today.AddDays(1)),
                        DueThisWeek = pendingRegistrations.Count(r =>
                            r.PaymentDeadline >= DateTime.Today.AddDays(1) &&
                            r.PaymentDeadline < DateTime.Today.AddDays(7))
                    },
                    PendingRegistrations = pendingRegistrations.Select(r => new
                    {
                        LearningRegisId = r.LearningRegisId,
                        LearnerId = r.LearnerId,
                        LearnerName = r.Learner?.FullName ?? "Unknown",
                        Status = r.Status.ToString(),
                        ReminderCount = r.ReminderCount,
                        LastReminderSent = r.LastReminderSent,
                        PaymentDeadline = r.PaymentDeadline,
                        IsOverdue = r.PaymentDeadline < DateTime.Now,
                        DaysUntilDeadline = r.PaymentDeadline.HasValue ?
                            (int)Math.Ceiling((r.PaymentDeadline.Value - DateTime.Now).TotalDays) : 0
                    }).ToList()
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã lấy thống kê nhắc nhở thanh toán cho {pendingRegistrations.Count} đăng ký",
                    Data = stats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment reminder statistics");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy thống kê nhắc nhở thanh toán: {ex.Message}"
                };
            }
        }
    }
}
