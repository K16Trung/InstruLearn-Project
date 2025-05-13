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
                var feedbacksToUpdate = new List<LearningRegisFeedback>();
                var registrationsToUpdate = new List<Learning_Registration>();

                foreach (var regis in learningRegs)
                {
                    // Check if there's already a feedback for this registration
                    var existingFeedback = allFeedbacks
                        .FirstOrDefault(f => f.LearningRegistrationId == regis.LearningRegisId);

                    // Add this check before creating feedback notifications
                    if (regis.HasPendingLearningPath)
                    {
                        continue; // Skip if learning path is still pending
                    }

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
                            AdditionalComments = "",
                            DeadlineDate = DateTime.Now.AddDays(1)
                        };

                        await _unitOfWork.LearningRegisFeedbackRepository.AddAsync(newFeedback);
                        await _unitOfWork.SaveChangeAsync();

                        // Now retrieve the created feedback with its ID
                        existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                            .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                        _logger.LogInformation($"Created new feedback record with ID {existingFeedback.FeedbackId} for learning registration {regis.LearningRegisId}");
                    }
                    else if (existingFeedback.DeadlineDate == null)
                    {
                        // Set deadline for existing feedbacks that don't have one
                        existingFeedback.DeadlineDate = DateTime.Now.AddDays(1);
                        feedbacksToUpdate.Add(existingFeedback);
                    }

                    // Check if feedback deadline has passed and update status if needed
                    if (existingFeedback.DeadlineDate.HasValue &&
                        DateTime.Now > existingFeedback.DeadlineDate.Value &&
                        existingFeedback.Status != FeedbackStatus.Completed)
                    {
                        _logger.LogInformation($"Feedback deadline passed for feedback ID {existingFeedback.FeedbackId}. Auto-updating status.");

                        // Mark feedback as completed automatically
                        existingFeedback.Status = FeedbackStatus.Completed;
                        existingFeedback.CompletedAt = DateTime.Now;
                        existingFeedback.AdditionalComments = "Auto-completed by system due to deadline expiration";

                        // Update the learning registration to FourtyFeedbackDone
                        if (regis.Status == LearningRegis.Fourty)
                        {
                            regis.Status = LearningRegis.FourtyFeedbackDone;
                            registrationsToUpdate.Add(regis);
                        }

                        feedbacksToUpdate.Add(existingFeedback);
                        continue; // Skip displaying this feedback since it's now completed
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
                            DeadlineDate = existingFeedback.DeadlineDate,
                            Questions = questions,
                            Message = $"Bạn đã thanh toán 40% học phí. Vui lòng hoàn thành phản hồi này để xác nhận bạn muốn tiếp tục học và thanh toán 60% còn lại."
                        });
                    }
                }

                // Save all feedback updates
                foreach (var feedback in feedbacksToUpdate)
                {
                    await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);
                }

                // Save all registration status updates
                foreach (var registration in registrationsToUpdate)
                {
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(registration);
                }

                if (feedbacksToUpdate.Any() || registrationsToUpdate.Any())
                {
                    await _unitOfWork.SaveChangeAsync();
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

                var learnersWithFortyStatus = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.Status == LearningRegis.Fourty,
                        "Teacher,Learner,Learner.Account,Schedules"
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

                var feedbackQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository
                    .GetActiveQuestionsWithOptionsAsync();

                foreach (var regis in learnersWithFortyStatus)
                {
                    try
                    {
                        int totalSessions = regis.NumberOfSession;
                        int fortyPercentThreshold = (int)Math.Ceiling(totalSessions * 0.4);

                        var completedSessions = regis.Schedules
                            ?.Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                      s.AttendanceStatus == AttendanceStatus.Absent) ?? 0;

                        if (completedSessions < fortyPercentThreshold || regis.HasPendingLearningPath)
                        {
                            continue;
                        }

                        var existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                            .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                        if (existingFeedback == null)
                        {
                            var newFeedback = new LearningRegisFeedback
                            {
                                LearningRegistrationId = regis.LearningRegisId,
                                LearnerId = regis.LearnerId,
                                CreatedAt = DateTime.Now,
                                Status = FeedbackStatus.NotStarted,
                                AdditionalComments = "",
                                DeadlineDate = DateTime.Now.AddDays(1)
                            };

                            await _unitOfWork.LearningRegisFeedbackRepository.AddAsync(newFeedback);
                            await _unitOfWork.SaveChangeAsync();

                            existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                                .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                            _logger.LogInformation($"Created new feedback record with ID {existingFeedback.FeedbackId} for learning registration {regis.LearningRegisId}");
                        }

                        if (existingFeedback != null &&
                            (existingFeedback.Status == FeedbackStatus.NotStarted ||
                             existingFeedback.Status == FeedbackStatus.InProgress))
                        {
                            if (regis.Learner != null && regis.Learner.Account != null && !string.IsNullOrEmpty(regis.Learner.Account.Email))
                            {
                                var learnerEmail = regis.Learner.Account.Email;
                                var learnerName = regis.Learner.FullName;

                                decimal remainingPayment = regis.Price.HasValue ? regis.Price.Value * 0.6m : 0;

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
                                    CompletedSessions = completedSessions,
                                    TotalSessions = totalSessions,
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

        public async Task<ResponseDTO> CheckAndUpdateLearnerProgressAsync()
        {
            try
            {
                _logger.LogInformation("Starting progress check for learners with Fourty status");

                // Get learning registrations with Fourty status - these are learners who have paid 40% and are learning
                var fortyStatusRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.Status == LearningRegis.Fourty,
                        "Teacher,Learner,Learner.Account,Schedules"
                    );

                if (fortyStatusRegistrations == null || !fortyStatusRegistrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No learning registrations with Fourty status found."
                    };
                }

                int notificationCount = 0;
                var results = new List<object>();

                // Process each registration
                foreach (var regis in fortyStatusRegistrations)
                {
                    try
                    {
                        if (regis.HasPendingLearningPath)
                        {
                            _logger.LogInformation($"Skipping registration {regis.LearningRegisId} because it has a pending learning path");
                            continue;
                        }
                        // Get completed sessions count (sessions marked as Present or Absent)
                        var completedSessions = regis.Schedules
                            .Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                      s.AttendanceStatus == AttendanceStatus.Absent);

                        // Total number of sessions
                        int totalSessions = regis.NumberOfSession;

                        // Skip if no sessions or total sessions is invalid
                        if (totalSessions <= 0 || completedSessions <= 0)
                        {
                            continue;
                        }

                        // Calculate the 40% threshold of total learning sessions (round up)
                        int fortyPercentThreshold = (int)Math.Ceiling(totalSessions * 0.4);

                        // Calculate progress percentage
                        double progressPercentage = (double)completedSessions / totalSessions * 100;

                        _logger.LogInformation($"Checking learner {regis.LearnerId} progress: {completedSessions}/{totalSessions} sessions " +
                                               $"({progressPercentage:F1}%), threshold: {fortyPercentThreshold} sessions");

                        // Check if the learner has completed at least 40% of their learning sessions
                        if (completedSessions >= fortyPercentThreshold)
                        {
                            _logger.LogInformation($"Learner {regis.LearnerId} has completed {completedSessions} out of {totalSessions} " +
                                                   $"sessions ({progressPercentage:F1}%), sending feedback notification");

                            // Check if feedback already exists for this registration
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

                                // Retrieve the created feedback with its ID
                                existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                                    .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                                _logger.LogInformation($"Created new feedback record with ID {existingFeedback.FeedbackId} for registration {regis.LearningRegisId}");
                            }

                            // Send notification if feedback is not completed yet
                            if (existingFeedback != null &&
                                (existingFeedback.Status == FeedbackStatus.NotStarted ||
                                 existingFeedback.Status == FeedbackStatus.InProgress))
                            {
                                // If the learner has an email, send a notification email
                                if (regis.Learner?.Account?.Email != null)
                                {
                                    var learnerEmail = regis.Learner.Account.Email;
                                    var learnerName = regis.Learner.FullName;

                                    // Calculate remaining payment (60% of total)
                                    decimal remainingPayment = regis.Price.HasValue ? regis.Price.Value * 0.6m : 0;

                                    // Send email notification
                                    await SendFeedbackEmailNotification(
                                        learnerEmail,
                                        learnerName,
                                        existingFeedback.FeedbackId,
                                        regis.Teacher?.Fullname ?? "your teacher",
                                        remainingPayment
                                    );

                                    notificationCount++;

                                    results.Add(new
                                    {
                                        LearningRegisId = regis.LearningRegisId,
                                        LearnerId = regis.LearnerId,
                                        LearnerName = regis.Learner.FullName,
                                        FeedbackId = existingFeedback.FeedbackId,
                                        TeacherName = regis.Teacher?.Fullname ?? "N/A",
                                        CompletedSessions = completedSessions,
                                        TotalSessions = totalSessions,
                                        ProgressPercentage = Math.Round(progressPercentage, 2),
                                        RemainingPayment = remainingPayment,
                                        EmailSent = true
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing progress check for registration {LearningRegisId}", regis.LearningRegisId);
                        results.Add(new
                        {
                            LearningRegisId = regis.LearningRegisId,
                            LearnerId = regis.LearnerId,
                            Error = ex.Message,
                            NotificationSent = false
                        });
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Processed {fortyStatusRegistrations.Count} registrations with Fourty status. Sent {notificationCount} feedback notifications.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in learner progress check process");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error in learner progress check process: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CheckForExpiredFeedbacksAsync()
        {
            try
            {
                _logger.LogInformation("Checking for expired feedback deadlines");

                // Get all feedbacks that are not completed and have a deadline date in the past
                var expiredFeedbacks = await _unitOfWork.LearningRegisFeedbackRepository
                    .GetWithIncludesAsync(
                        f => f.Status != FeedbackStatus.Completed &&
                             f.DeadlineDate.HasValue &&
                             f.DeadlineDate < DateTime.Now,
                        "LearningRegistration"
                    );

                if (expiredFeedbacks == null || !expiredFeedbacks.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No expired feedbacks found."
                    };
                }

                int updatedCount = 0;
                var results = new List<object>();

                foreach (var feedback in expiredFeedbacks)
                {
                    try
                    {
                        // Update feedback status
                        feedback.Status = FeedbackStatus.Completed;
                        feedback.CompletedAt = DateTime.Now;
                        feedback.AdditionalComments = (feedback.AdditionalComments ?? "") +
                                                     "\nAuto-completed by system due to deadline expiration";

                        await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);

                        // Get and update the learning registration
                        var learningRegis = feedback.LearningRegistration;
                        if (learningRegis == null)
                        {
                            learningRegis = await _unitOfWork.LearningRegisRepository
                                .GetByIdAsync(feedback.LearningRegistrationId);
                        }

                        if (learningRegis != null && learningRegis.Status == LearningRegis.Fourty)
                        {
                            learningRegis.Status = LearningRegis.FourtyFeedbackDone;
                            await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                        }

                        updatedCount++;

                        results.Add(new
                        {
                            FeedbackId = feedback.FeedbackId,
                            LearningRegistrationId = feedback.LearningRegistrationId,
                            LearnerId = feedback.LearnerId,
                            ExpiredOn = feedback.DeadlineDate,
                            ProcessedOn = DateTime.Now
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing expired feedback {FeedbackId}", feedback.FeedbackId);
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Updated {updatedCount} expired feedbacks.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for expired feedbacks");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking for expired feedbacks: {ex.Message}"
                };
            }
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
                            <a href='http://localhost:3000/notification' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
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
