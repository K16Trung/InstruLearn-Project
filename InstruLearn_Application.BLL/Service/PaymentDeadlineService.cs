using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
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
    public class PaymentDeadlineService : BackgroundService
    {
        private readonly ILogger<PaymentDeadlineService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Check every hour
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public PaymentDeadlineService(
            ILogger<PaymentDeadlineService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Deadline Service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPaymentDeadlinesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing payment deadlines");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessPaymentDeadlinesAsync()
        {
            _logger.LogInformation("Checking for expired payment deadlines at: {time}", DateTimeOffset.Now);

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Get all learning registrations with status 'Accepted' or 'FourtyFeedbackDone'
            var pendingRegistrations = await unitOfWork.LearningRegisRepository
                .GetWithIncludesAsync(
                    x => (x.Status == LearningRegis.Accepted || x.Status == LearningRegis.FourtyFeedbackDone) &&
                         x.PaymentDeadline.HasValue,
                    "Learner,Learner.Account,Teacher,Schedules"
                );

            int rejectedCount = 0;

            foreach (var registration in pendingRegistrations)
            {
                // If payment deadline has passed
                if (DateTime.Now > registration.PaymentDeadline)
                {
                    _logger.LogInformation("Processing expired payment for registration ID: {id}, LearnerId: {learnerId}, Status: {status}",
                        registration.LearningRegisId, registration.LearnerId, registration.Status);

                    try
                    {
                        using var transaction = await unitOfWork.BeginTransactionAsync();

                        // Update status to rejected or cancelled based on current state
                        if (registration.Status == LearningRegis.Accepted)
                        {
                            registration.Status = LearningRegis.Cancelled;
                            registration.LearningRequest = "Quá hạn thanh toán";
                        }
                        else if (registration.Status == LearningRegis.FourtyFeedbackDone)
                        {
                            registration.Status = LearningRegis.Cancelled;
                            registration.LearningRequest = "Quá hạn thanh toán 60%";

                            // Get all schedules associated with this learning registration
                            var schedules = registration.Schedules?.ToList() ??
                                await unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(registration.LearningRegisId);

                            // Delete all schedules for this learning registration
                            foreach (var schedule in schedules)
                            {
                                await unitOfWork.ScheduleRepository.DeleteAsync(schedule.ScheduleId);
                            }
                        }

                        await unitOfWork.LearningRegisRepository.UpdateAsync(registration);


                        await unitOfWork.SaveChangeAsync();

                        // Send notification emails
                        if (registration.Learner?.Account?.Email != null)
                        {
                            try
                            {
                                string status = registration.Status == LearningRegis.Cancelled ? "Đã từ chối" : "Đã hủy";
                                string subject = $"Đăng ký học của bạn đã bị {status} do hết hạn thanh toán";
                                string body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                <h2 style='color: #333;'>Xin chào {registration.Learner.FullName},</h2>
                                
                                <p>Chúng tôi rất tiếc phải thông báo rằng đăng ký học của bạn đã bị {status.ToLower()} do không thanh toán học phí đúng hạn.</p>
                                
                                <div style='background-color: #fff0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff5252;'>
                                    <h3 style='margin-top: 0; color: #333;'>Thông tin đăng ký:</h3>
                                    <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
                                    <p><strong>Giáo viên:</strong> {registration.Teacher?.Fullname ?? "N/A"}</p>
                                    <p><strong>Hạn thanh toán:</strong> {registration.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm")}</p>
                                </div>
                                
                                <p>Nếu bạn vẫn muốn tiếp tục học, vui lòng tạo đăng ký mới.</p>
                                
                                <p>Nếu bạn cho rằng đây là sai sót, vui lòng liên hệ ngay với đội ngũ hỗ trợ của chúng tôi.</p>
                                
                                <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                            </div>
                        </body>
                        </html>";

                                await emailService.SendEmailAsync(registration.Learner.Account.Email, subject, body, true);
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, "Failed to send email notification to learner for registration {id}", registration.LearningRegisId);
                            }
                        }

                        // Notify teacher if it's a FourtyFeedbackDone status
                        if (registration.Status == LearningRegis.Cancelled &&
                            registration.Teacher?.AccountId != null &&
                            registration.Schedules?.Any() == true)
                        {
                            var teacherAccount = await unitOfWork.AccountRepository.GetByIdAsync(registration.Teacher.AccountId);
                            if (teacherAccount != null && !string.IsNullOrEmpty(teacherAccount.Email))
                            {
                                try
                                {
                                    string subject = "Đăng ký học đã bị hủy tự động";
                                    string body = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                    <h2 style='color: #333;'>Xin chào {registration.Teacher.Fullname},</h2>
                                    
                                    <p>Đăng ký học của học viên {registration.Learner?.FullName ?? "N/A"} đã bị hủy tự động do không thanh toán 60% học phí còn lại đúng hạn.</p>
                                    
                                    <p>Tất cả lịch học liên quan đến đăng ký này đã bị xóa khỏi lịch dạy của bạn.</p>
                                    
                                    <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                                        <h3 style='margin-top: 0; color: #333;'>Thông tin đăng ký:</h3>
                                        <p><strong>ID đăng ký học:</strong> {registration.LearningRegisId}</p>
                                        <p><strong>Học viên:</strong> {registration.Learner?.FullName ?? "N/A"}</p>
                                    </div>
                                    
                                    <p>Vui lòng kiểm tra lịch dạy của bạn để cập nhật thông tin mới nhất.</p>
                                    
                                    <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                                </div>
                            </body>
                            </html>";

                                    await emailService.SendEmailAsync(teacherAccount.Email, subject, body, true);
                                }
                                catch (Exception emailEx)
                                {
                                    _logger.LogError(emailEx, "Failed to send email notification to teacher for registration {id}", registration.LearningRegisId);
                                }
                            }
                        }

                        await transaction.CommitAsync();

                        rejectedCount++;

                        _logger.LogInformation("Successfully processed expired payment for registration ID: {id} - New status: {status}",
                            registration.LearningRegisId, registration.Status);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process expired payment for registration ID: {id}", registration.LearningRegisId);
                    }
                }
            }

            _logger.LogInformation("Payment deadline check completed. Processed {count} registrations with expired deadlines", rejectedCount);
        }
    }
}
