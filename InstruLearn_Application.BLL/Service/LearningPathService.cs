using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningPathService : ILearningPathService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LearningPathService> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public LearningPathService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LearningPathService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> GetLearningPathSessionsAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                var learningPathSessions = await _unitOfWork.LearningPathSessionRepository.GetByLearningRegisIdAsync(learningRegisId);
                var learningPathSessionDTOs = _mapper.Map<List<LearningPathSessionDTO>>(learningPathSessions);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learning path sessions retrieved successfully.",
                    Data = learningPathSessionDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning path sessions for registration {LearningRegisId}", learningRegisId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to retrieve learning path sessions: " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> UpdateSessionCompletionStatusAsync(int learningPathSessionId, bool isCompleted)
        {
            try
            {
                var session = await _unitOfWork.LearningPathSessionRepository.GetByIdAsync(learningPathSessionId);

                if (session == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning path session not found."
                    };
                }

                // Update completion status
                session.IsCompleted = isCompleted;
                await _unitOfWork.LearningPathSessionRepository.UpdateAsync(session);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Session completion status {(isCompleted ? "marked as completed" : "marked as incomplete")} successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning path session completion status");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update session completion status: " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> UpdateLearningPathSessionAsync(UpdateLearningPathSessionDTO updateDTO)
        {
            try
            {
                // Check if the learning path session exists
                var session = await _unitOfWork.LearningPathSessionRepository.GetByIdAsync(updateDTO.LearningPathSessionId);
                if (session == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning path session not found."
                    };
                }

                // Update the session properties
                session.SessionNumber = updateDTO.SessionNumber;
                session.Title = updateDTO.Title;
                session.Description = updateDTO.Description;
                session.IsCompleted = updateDTO.IsCompleted;

                await _unitOfWork.LearningPathSessionRepository.UpdateAsync(session);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learning path session updated successfully.",
                    Data = new
                    {
                        LearningPathSessionId = session.LearningPathSessionId,
                        SessionNumber = session.SessionNumber,
                        Title = session.Title,
                        Description = session.Description,
                        IsCompleted = session.IsCompleted
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning path session: {Message}", ex.Message);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update learning path session: " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> ConfirmLearningPathAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                // Get the learning path sessions
                var sessions = await _unitOfWork.LearningPathSessionRepository
                    .GetByLearningRegisIdAsync(learningRegisId);

                if (!sessions.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No learning path sessions found for this registration. Please create sessions before confirming."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Make all sessions visible to the learner
                    foreach (var session in sessions)
                    {
                        session.IsVisible = true;
                        session.IsCompleted = true;
                        await _unitOfWork.LearningPathSessionRepository.UpdateAsync(session);
                    }

                    await _unitOfWork.SaveChangeAsync();

                    // Update the registration to indicate it no longer has pending sessions
                    learningRegis.HasPendingLearningPath = false;

                    // Set payment deadline here (moved from UpdateLearningRegisStatusAsync)
                    learningRegis.PaymentDeadline = DateTime.Now.AddDays(3);

                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    // Find and mark any related CreateLearningPath notifications as resolved
                    var notifications = await _unitOfWork.StaffNotificationRepository
                        .GetQuery()
                        .Where(n => n.LearningRegisId == learningRegisId &&
                                   n.Type == NotificationType.CreateLearningPath &&
                                   n.Status != NotificationStatus.Resolved)
                        .ToListAsync();

                    if (notifications.Any())
                    {
                        _logger.LogInformation($"Marking {notifications.Count} CreateLearningPath notifications as resolved for learning registration {learningRegisId}");

                        foreach (var notification in notifications)
                        {
                            notification.Status = NotificationStatus.Resolved;
                            await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                        }

                        await _unitOfWork.SaveChangeAsync();
                    }

                    // Get learner details for notification
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);
                    var account = learner != null ? await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId) : null;

                    // Calculate payment information (40% of total price)
                    decimal totalPrice = learningRegis.Price ?? 0;
                    decimal paymentAmount = totalPrice * 0.4m;
                    var deadline = learningRegis.PaymentDeadline?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";

                    // Send notification to learner if email is available
                    if (account != null && !string.IsNullOrEmpty(account.Email))
                    {
                        try
                        {
                            // Construct email content with payment details
                            string subject = "Lộ trình học tập của bạn đã được cập nhật";
                            string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                            <h2 style='color: #333;'>Xin chào {learner.FullName},</h2>
                            
                            <p>Chúng tôi vui mừng thông báo rằng giáo viên của bạn đã tạo một lộ trình học tập cho bạn.</p>
                            
                            <p>Lộ trình học tập của bạn bao gồm {sessions.Count} buổi học được thiết kế để giúp bạn đạt được mục tiêu học tập của mình.</p>
                            
                            <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
                                <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
                                <p><strong>Tổng số tiền:</strong> {totalPrice:N0} VND</p>
                                <p><strong>Thanh toán cần thiết (40%):</strong> {paymentAmount:N0} VND</p>
                                <p><strong>Hạn thanh toán:</strong> {deadline}</p>
                            </div>
                            
                            <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                                <a href='http://localhost:3000/profile/registration-detail/{learningRegisId}?scrollToLearningPath=true' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                                    Xem Lộ Trình Học Tập Của Bạn
                                </a>
                            </div>
                            
                            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với giáo viên của bạn hoặc nhóm hỗ trợ của chúng tôi.</p>
                            
                            <p>Chúc bạn học tập hiệu quả!</p>
                            
                            <p>Trân trọng,<br>Nhóm InstruLearn</p>
                        </div>
                    </body>
                    </html>";

                            // Use the injected email service with isHtml parameter set to true
                            await _emailService.SendEmailAsync(
                                account.Email,
                                subject,
                                body,
                                isHtml: true
                            );
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "Error sending learning path notification email");
                            // Continue even if email fails
                        }
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Learning path confirmed and is now visible to the learner.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            SessionCount = sessions.Count,
                            PaymentDeadline = deadline,
                            PaymentAmount = paymentAmount,
                            TotalPrice = totalPrice
                        }
                    };
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error confirming learning path: {Message}", ex.Message);
                    throw; // Re-throw to be caught by outer catch
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming learning path: {Message}", ex.Message);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to confirm learning path: {ex.Message}"
                };
            }
        }
    }
}
