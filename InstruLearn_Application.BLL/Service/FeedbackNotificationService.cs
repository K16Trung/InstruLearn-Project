using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class FeedbackNotificationService : IFeedbackNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FeedbackNotificationService> _logger;
        private readonly IEmailService _emailService;

        public FeedbackNotificationService(IUnitOfWork unitOfWork, ILogger<FeedbackNotificationService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> CheckLearnerFeedbackNotificationsAsync(int learnerId)
        {
            try
            {
                _logger.LogInformation($"Checking feedback notifications for learner ID: {learnerId}");

                // Get active learning registrations for this learner with status Fourty
                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearnerId == learnerId && x.Status == LearningRegis.Fourty,
                        "Teacher,Schedules"
                    );

                if (learningRegs == null || !learningRegs.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No active learning registrations found for this learner."
                    };
                }

                // Get all feedback forms for this learner (both completed and not completed)
                var allFeedbacks = await _unitOfWork.LearningRegisFeedbackRepository
                    .GetFeedbacksByLearnerIdAsync(learnerId);

                // Get all active feedback questions (only once, as questions are shared)
                var feedbackQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository
                    .GetActiveQuestionsWithOptionsAsync();

                var feedbackNotifications = new List<object>();

                foreach (var regis in learningRegs)
                {
                    // Check if there's already a feedback for this registration
                    var existingFeedback = allFeedbacks
                        .FirstOrDefault(f => f.LearningRegistrationId == regis.LearningRegisId);

                    // If no feedback exists, create one for the learner
                    if (existingFeedback == null)
                    {
                        // Create a new feedback record (not a new form)
                        var newFeedback = new LearningRegisFeedback
                        {
                            LearningRegistrationId = regis.LearningRegisId,
                            LearnerId = learnerId,
                            CreatedAt = DateTime.Now,
                            Status = FeedbackStatus.NotStarted,
                            AdditionalComments = ""
                        };

                        await _unitOfWork.LearningRegisFeedbackRepository.AddAsync(newFeedback);
                        await _unitOfWork.SaveChangeAsync();

                        // Now retrieve the created feedback with its ID
                        existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                            .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                        _logger.LogInformation($"Created new feedback record with ID {existingFeedback.FeedbackId} for learning registration {regis.LearningRegisId}");
                    }

                    // Only include notifications for forms that are not completed
                    if (existingFeedback != null &&
                        (existingFeedback.Status == FeedbackStatus.NotStarted ||
                         existingFeedback.Status == FeedbackStatus.InProgress))
                    {
                        // Get completed sessions count
                        var completedSessions = regis.Schedules
                            .Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                      s.AttendanceStatus == AttendanceStatus.Absent);

                        // Total number of sessions
                        int totalSessions = regis.NumberOfSession;

                        // Calculate progress percentage
                        double progressPercentage = totalSessions > 0
                            ? (double)completedSessions / totalSessions * 100
                            : 0;

                        // Calculate remaining payment amount (60% of total)
                        decimal remainingPayment = 0;
                        if (regis.Price.HasValue)
                        {
                            remainingPayment = regis.Price.Value * 0.6m;
                        }

                        // Create a notification with shared questions and without circular references
                        var questions = feedbackQuestions.Select(q => new
                        {
                            q.QuestionId,
                            q.QuestionText,
                            q.DisplayOrder,
                            q.IsRequired,
                            q.IsActive,
                            Options = q.Options.Select(o => new
                            {
                                o.OptionId,
                                o.OptionText,
                                o.QuestionId
                                // No reference back to Question
                            }).ToList()
                        }).ToList();

                        feedbackNotifications.Add(new
                        {
                            FeedbackId = existingFeedback.FeedbackId,
                            LearningRegisId = regis.LearningRegisId,
                            TeacherId = regis.TeacherId,
                            TeacherName = regis.Teacher?.Fullname ?? "N/A",
                            TotalSessions = totalSessions,
                            CompletedSessions = completedSessions,
                            ProgressPercentage = Math.Round(progressPercentage, 2),
                            TotalPrice = regis.Price,
                            RemainingPayment = remainingPayment,
                            FeedbackStatus = existingFeedback.Status.ToString(),
                            CreatedAt = existingFeedback.CreatedAt,
                            Questions = questions,
                            Message = $"Bạn đã thanh toán 40% học phí. Vui lòng hoàn thành phản hồi này để xác nhận bạn muốn tiếp tục học và thanh toán 60% còn lại."
                        });
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = feedbackNotifications.Any()
                        ? "Feedback notifications available."
                        : "No feedback notifications at this time.",
                    Data = feedbackNotifications
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feedback notifications for learner {LearnerId}", learnerId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking feedback notifications: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> ProcessFeedbackCompletionAsync(int feedbackId, bool continueStudying)
        {
            try
            {
                var feedback = await _unitOfWork.LearningRegisFeedbackRepository
                    .GetFeedbackWithDetailsAsync(feedbackId);

                if (feedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Feedback form not found."
                    };
                }

                // Mark feedback as completed
                feedback.Status = FeedbackStatus.Completed;
                feedback.CompletedAt = DateTime.Now;
                await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);
                await _unitOfWork.SaveChangeAsync();

                // Get the learning registration
                var learningRegis = await _unitOfWork.LearningRegisRepository
                    .GetByIdAsync(feedback.LearningRegistrationId);

                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning registration not found."
                    };
                }

                // If the learner wants to continue studying
                if (continueStudying)
                {
                    // Update learning registration status to indicate readiness for 60% payment
                    if (learningRegis.Status == LearningRegis.Fourty)
                    {
                        learningRegis.Status = LearningRegis.FourtyFeedbackDone;
                        await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Feedback completed. Your learning registration is now ready for the remaining 60% payment to continue learning.",
                        Data = new
                        {
                            FeedbackId = feedbackId,
                            LearningRegisId = learningRegis.LearningRegisId,
                            Status = learningRegis.Status.ToString(),
                            RemainingPayment = learningRegis.Price.HasValue ? learningRegis.Price.Value * 0.6m : 0
                        }
                    };
                }
                else
                {
                    // Learner doesn't want to continue
                    learningRegis.Status = LearningRegis.Cancelled;
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Feedback completed. Your learning registration has been cancelled as requested.",
                        Data = new
                        {
                            FeedbackId = feedbackId,
                            LearningRegisId = learningRegis.LearningRegisId,
                            Status = learningRegis.Status.ToString()
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing feedback completion for feedback {FeedbackId}", feedbackId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error processing feedback completion: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> AutoCheckAndCreateFeedbackNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting automatic feedback notification check");

                // Get all learning registrations with 40% status 
                var learnersWithFortyStatus = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.Status == LearningRegis.Fourty,
                        "Teacher,Learner,Learner.Account"
                    );

                if (learnersWithFortyStatus == null || !learnersWithFortyStatus.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No learners with 40% payment status found."
                    };
                }

                int notificationsSent = 0;
                var results = new List<object>();

                // Get all active feedback questions
                var feedbackQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository
                    .GetActiveQuestionsWithOptionsAsync();

                // Process each registration
                foreach (var regis in learnersWithFortyStatus)
                {
                    try
                    {
                        // Check if there's already a feedback form for this registration
                        var existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                            .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                        // If no feedback exists, create one
                        if (existingFeedback == null)
                        {
                            var newFeedback = new LearningRegisFeedback
                            {
                                LearningRegistrationId = regis.LearningRegisId,
                                LearnerId = regis.LearnerId,
                                CreatedAt = DateTime.Now,
                                Status = FeedbackStatus.NotStarted,
                                AdditionalComments = ""
                            };

                            await _unitOfWork.LearningRegisFeedbackRepository.AddAsync(newFeedback);
                            await _unitOfWork.SaveChangeAsync();

                            // Now retrieve the created feedback with its ID
                            existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                                .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                            _logger.LogInformation($"Created new feedback record with ID {existingFeedback.FeedbackId} for learning registration {regis.LearningRegisId}");
                        }

                        // Only send notifications for forms that are not completed
                        if (existingFeedback != null &&
                            (existingFeedback.Status == FeedbackStatus.NotStarted ||
                             existingFeedback.Status == FeedbackStatus.InProgress))
                        {
                            // If the learner has an email, send a notification email
                            if (regis.Learner != null && regis.Learner.Account != null && !string.IsNullOrEmpty(regis.Learner.Account.Email))
                            {
                                var learnerEmail = regis.Learner.Account.Email;
                                var learnerName = regis.Learner.FullName;

                                // Calculate remaining payment
                                decimal remainingPayment = regis.Price.HasValue ? regis.Price.Value * 0.6m : 0;

                                // Send email notification
                                await SendFeedbackEmailNotification(
                                    learnerEmail,
                                    learnerName,
                                    existingFeedback.FeedbackId,
                                    regis.Teacher?.Fullname ?? "your teacher",
                                    remainingPayment
                                );

                                notificationsSent++;

                                results.Add(new
                                {
                                    LearningRegisId = regis.LearningRegisId,
                                    LearnerId = regis.LearnerId,
                                    LearnerName = regis.Learner.FullName,
                                    FeedbackId = existingFeedback.FeedbackId,
                                    EmailSent = true
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing feedback for registration {LearningRegisId}", regis.LearningRegisId);
                        results.Add(new
                        {
                            LearningRegisId = regis.LearningRegisId,
                            LearnerId = regis.LearnerId,
                            Error = ex.Message,
                            EmailSent = false
                        });
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Processed {learnersWithFortyStatus.Count} registrations with 40% payment status. Sent {notificationsSent} email notifications.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in automatic feedback notification process");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error in automatic feedback notification process: {ex.Message}"
                };
            }
        }

        public async Task SendTestFeedbackEmailNotification(string email, string learnerName, int feedbackId, string teacherName, decimal remainingPayment)
        {
            _logger.LogInformation($"Sending test feedback email notification to {email}");

            // Using the existing email notification method
            await SendFeedbackEmailNotification(
                email,
                learnerName,
                feedbackId,
                teacherName,
                remainingPayment
            );

            _logger.LogInformation($"Test email notification sent successfully to {email}");
        }

        private async Task SendFeedbackEmailNotification(string email, string learnerName, int feedbackId, string teacherName, decimal remainingPayment)
        {
            string subject = "Feedback Required: Continue Your Learning Journey";

            string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333;'>Xin chào {learnerName},</h2>
                        
                        <p>Cảm ơn bạn đã tiếp tục học cùng chúng tôi. Chúng tôi hy vọng bạn thích các lớp học của {teacherName}.</p>
                        
                        <p>Bạn đã đạt đến 40% hành trình học tập của mình và chúng tôi muốn nhận phản hồi của bạn trước khi bạn tiếp tục.</p>
                        
                        <p>Vui lòng điền vào phản hồi này để cho chúng tôi biết nếu bạn muốn tiếp tục 60% còn lại của chương trình học.</p>
                        
                        <p>Thanh toán còn lại: {remainingPayment.ToString("N0")} VND</p>
                        
                        <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                            <a href='https://instrulearn.com/feedback/{feedbackId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                                Hoàn thành Biểu mẫu phản hồi
                            </a>
                        </div>
                        
                        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi.</p>
                        
                        <p>Trân trọng,<br>Nhóm InstruLearn</p>
                    </div>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }
    }
}
