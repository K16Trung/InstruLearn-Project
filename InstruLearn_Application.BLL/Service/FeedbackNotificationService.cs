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

                // Get all completed feedbacks for this learner regardless of registration status
                var allFeedbacks = await _unitOfWork.LearningRegisFeedbackRepository
                    .GetFeedbacksByLearnerIdAsync(learnerId);

                // Get active learning registrations (40%, post-feedback, or 60%)
                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearnerId == learnerId && (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.FourtyFeedbackDone || x.Status == LearningRegis.Sixty),
                        "Teacher,Schedules"
                    );

                var feedbackNotifications = new List<object>();
                var feedbacksToUpdate = new List<LearningRegisFeedback>();
                var registrationsToUpdate = new List<Learning_Registration>();

                // No active learning registrations or feedbacks
                if ((learningRegs == null || !learningRegs.Any()) &&
                    (allFeedbacks == null || !allFeedbacks.Any()))
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No active learning registrations or feedback records found for this learner."
                    };
                }

                // Get feedback questions for all notifications
                var feedbackQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository
                    .GetActiveQuestionsWithOptionsAsync();

                // Format questions to avoid circular references
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
                    }).ToList()
                }).ToList();

                // First, handle all completed feedbacks to ensure they're always included
                foreach (var completedFeedback in allFeedbacks.Where(f => f.Status == FeedbackStatus.Completed))
                {
                    // Get the registration for this feedback
                    var regis = learningRegs?.FirstOrDefault(r => r.LearningRegisId == completedFeedback.LearningRegistrationId);

                    // If the registration doesn't exist in our active list, fetch it directly
                    if (regis == null)
                    {
                        regis = await _unitOfWork.LearningRegisRepository
                            .GetWithIncludesAsync(
                                x => x.LearningRegisId == completedFeedback.LearningRegistrationId,
                                "Teacher,Schedules"
                            ).ContinueWith(t => t.Result?.FirstOrDefault());

                        if (regis == null)
                        {
                            _logger.LogWarning($"Could not find registration for completed feedback ID {completedFeedback.FeedbackId}");
                            continue;
                        }
                    }

                    // Calculate session information
                    var completedSessions = regis.Schedules
                        ?.Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                  s.AttendanceStatus == AttendanceStatus.Absent) ?? 0;

                    int totalSessions = regis.NumberOfSession;
                    double progressPercentage = totalSessions > 0
                        ? (double)completedSessions / totalSessions * 100
                        : 0;

                    // Get feedback answers
                    var feedbackAnswers = await _unitOfWork.LearningRegisFeedbackAnswerRepository
                        .GetAnswersByFeedbackIdAsync(completedFeedback.FeedbackId);

                    decimal totalPrice = regis.Price ?? 0;
                    decimal remainingPayment = 0;
                    if (regis.Status == LearningRegis.FourtyFeedbackDone)
                    {
                        remainingPayment = Math.Round(totalPrice * 0.6m, 2);
                    }

                    // Add the completed feedback notification
                    feedbackNotifications.Add(new
                    {
                        FeedbackId = completedFeedback.FeedbackId,
                        LearningRegisId = regis.LearningRegisId,
                        TeacherId = regis.TeacherId,
                        TeacherName = regis.Teacher?.Fullname ?? "N/A",
                        TotalSessions = totalSessions,
                        CompletedSessions = completedSessions,
                        ProgressPercentage = Math.Round(progressPercentage, 2),
                        FeedbackStatus = completedFeedback.Status.ToString(),
                        CreatedAt = completedFeedback.CreatedAt,
                        CompletedAt = completedFeedback.CompletedAt,
                        IsCompleted = true,
                        TotalPrice = totalPrice,
                        RemainingPayment = remainingPayment,
                        Questions = questions,
                        Answers = feedbackAnswers.Select(a => new
                        {
                            a.AnswerId,
                            a.QuestionId,
                            a.SelectedOptionId
                        }).ToList(),
                        Message = "Cảm ơn bạn đã hoàn thành phản hồi. Hãy tiếp tục thanh toán 60% còn lại để hoàn thành khóa học của bạn."
                    });

                    _logger.LogInformation($"Added completed feedback notification for feedback ID {completedFeedback.FeedbackId}, registration {regis.LearningRegisId}");
                }

                // Now process active learning registrations for non-completed feedbacks
                if (learningRegs != null && learningRegs.Any())
                {
                    foreach (var regis in learningRegs)
                    {
                        _logger.LogInformation($"Processing registration {regis.LearningRegisId} for learner {learnerId}");

                        var completedSessions = regis.Schedules
                            ?.Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                      s.AttendanceStatus == AttendanceStatus.Absent) ?? 0;

                        int totalSessions = regis.NumberOfSession;
                        int fortyPercentThreshold = Math.Max(1, (int)Math.Ceiling(totalSessions * 0.4));
                        double progressPercentage = totalSessions > 0
                            ? (double)completedSessions / totalSessions * 100
                            : 0;

                        var existingFeedback = allFeedbacks
                            .FirstOrDefault(f => f.LearningRegistrationId == regis.LearningRegisId);

                        // Skip if already processed as completed
                        if (existingFeedback != null && existingFeedback.Status == FeedbackStatus.Completed)
                        {
                            continue;
                        }

                        // Skip if below threshold and no completed feedback
                        if (completedSessions < fortyPercentThreshold)
                        {
                            _logger.LogInformation($"Skipping registration {regis.LearningRegisId} - below 40% threshold");
                            continue;
                        }

                        // The rest of your existing code for non-completed feedback goes here
                        // (creating new feedback records, checking deadlines, etc.)
                        if (existingFeedback == null)
                        {
                            // Create a new feedback record
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

                            existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                                .GetFeedbackByRegistrationIdAsync(regis.LearningRegisId);

                            _logger.LogInformation($"Created new feedback record with ID {existingFeedback.FeedbackId}");
                        }
                        else if (existingFeedback.DeadlineDate == null)
                        {
                            existingFeedback.DeadlineDate = DateTime.Now.AddDays(1);
                            feedbacksToUpdate.Add(existingFeedback);
                        }

                        // Check for expired deadline
                        if (existingFeedback.DeadlineDate.HasValue &&
                            DateTime.Now > existingFeedback.DeadlineDate.Value &&
                            existingFeedback.Status != FeedbackStatus.Completed)
                        {
                            _logger.LogInformation($"Feedback deadline passed for ID {existingFeedback.FeedbackId}");

                            existingFeedback.Status = FeedbackStatus.Completed;
                            existingFeedback.CompletedAt = DateTime.Now;
                            existingFeedback.AdditionalComments = "Auto-completed by system due to deadline expiration";

                            if (regis.Status == LearningRegis.Fourty)
                            {
                                regis.Status = LearningRegis.FourtyFeedbackDone;
                                registrationsToUpdate.Add(regis);
                            }

                            feedbacksToUpdate.Add(existingFeedback);
                            continue;
                        }

                        // Add notification for feedback in progress
                        if (existingFeedback.Status == FeedbackStatus.NotStarted ||
                            existingFeedback.Status == FeedbackStatus.InProgress)
                        {
                            decimal remainingPayment = 0;
                            if (regis.Price.HasValue)
                            {
                                remainingPayment = regis.Price.Value * 0.6m;
                            }

                            // Calculate deadline information
                            int daysRemaining = 0;
                            string deadlineMessage = "";

                            if (existingFeedback.DeadlineDate.HasValue)
                            {
                                TimeSpan timeRemaining = existingFeedback.DeadlineDate.Value - DateTime.Now;
                                daysRemaining = Math.Max(0, (int)Math.Ceiling(timeRemaining.TotalDays));

                                // Create deadline message (keep the existing logic)
                                if (daysRemaining < 1)
                                {
                                    int hoursRemaining = Math.Max(0, (int)Math.Ceiling(timeRemaining.TotalHours));
                                    deadlineMessage = hoursRemaining < 1
                                        ? "Hạn chót hôm nay! Vui lòng hoàn thành ngay."
                                        : $"Còn {hoursRemaining} giờ để hoàn thành phản hồi này.";
                                }
                                else
                                {
                                    deadlineMessage = daysRemaining == 1
                                        ? "Còn 1 ngày để hoàn thành phản hồi này."
                                        : $"Còn {daysRemaining} ngày để hoàn thành phản hồi này.";
                                }
                            }

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
                                DaysRemaining = daysRemaining,
                                DeadlineMessage = deadlineMessage,
                                Questions = questions,
                                Message = $"Bạn đã thanh toán 40% học phí. Vui lòng hoàn thành phản hồi này để xác nhận bạn muốn tiếp tục học và thanh toán 60% còn lại."
                            });
                        }
                    }
                }

                // Save all updates
                foreach (var feedback in feedbacksToUpdate)
                {
                    await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);
                }

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

                feedback.Status = FeedbackStatus.Completed;
                feedback.CompletedAt = DateTime.Now;
                await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);
                await _unitOfWork.SaveChangeAsync();

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

                if (continueStudying)
                {
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

                        int fortyPercentThreshold = Math.Max(1, (int)Math.Ceiling(totalSessions * 0.4));

                        var completedSessions = regis.Schedules
                            ?.Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                      s.AttendanceStatus == AttendanceStatus.Absent) ?? 0;

                        if (completedSessions < fortyPercentThreshold)
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
                        int fortyPercentThreshold = Math.Max(1, (int)Math.Ceiling(totalSessions * 0.4));

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
                                    AdditionalComments = "",
                                    DeadlineDate = DateTime.Now.AddDays(1)
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

        public async Task<ResponseDTO> CheckForClassLastDayFeedbacksAsync(bool includeOlderClasses = false)
        {
            try
            {
                _logger.LogInformation("Starting automatic check for classes on their last day or recently ended without feedback");
                _logger.LogInformation($"Including older classes: {includeOlderClasses}");

                var today = DateOnly.FromDateTime(DateTime.Today);
                var sevenDaysAgo = today.AddDays(-7);

                var registrationsWithType1002 = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(x => x.Learning_Registration_Type.RegisTypeId == 1002, "Classes");

                var classIdsWithType1002 = registrationsWithType1002
                    .Where(r => r.Classes != null)
                    .Select(r => r.Classes.ClassId)
                    .Distinct()
                    .ToList();

                _logger.LogInformation($"Found {classIdsWithType1002.Count} classes linked to registrations with regisTypeId 1002"); ;

                if (!classIdsWithType1002.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No classes with registration type 1002 found for feedback creation.",
                        Data = new List<object>()
                    };
                }

                var classes = await _unitOfWork.ClassRepository
                    .GetWithIncludesAsync(
                        x => (x.Status == ClassStatus.Scheduled || x.Status == ClassStatus.Ongoing || x.Status == ClassStatus.Completed),
                        "ClassDays,Teacher,Major,Level,Learner_Classes,Learner_Classes.Learner"
                    );

                if (classes == null || !classes.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No active or recently completed classes found for feedback creation."
                    };
                }

                int feedbacksCreated = 0;
                var classesProcessed = new List<object>();

                foreach (var classEntity in classes)
                {
                    try
                    {
                        var classDayValues = classEntity.ClassDays.Select(cd => cd.Day).ToList();
                        if (!classDayValues.Any()) continue;

                        var endDate = DateTimeHelper.CalculateEndDate(classEntity.StartDate, classEntity.totalDays, classDayValues);

                        // Check if the class is on its last day or has recently ended without feedback
                        bool isLastDay = endDate == today;
                        bool isRecentlyEnded = endDate < today && (includeOlderClasses || endDate >= sevenDaysAgo);

                        if (isLastDay || isRecentlyEnded)
                        {
                            _logger.LogInformation($"Class ID {classEntity.ClassId} '{classEntity.ClassName}' " +
                                                  (isLastDay ? "has its last day today." : "has recently ended without complete feedback."));

                            if (classEntity.LevelId == null)
                            {
                                _logger.LogWarning($"Class {classEntity.ClassId} has no level assigned, skipping feedback creation");
                                continue;
                            }

                            var template = await _unitOfWork.LevelFeedbackTemplateRepository
                                .GetTemplateForLevelAsync(classEntity.LevelId.Value);

                            if (template == null)
                            {
                                _logger.LogWarning($"No active feedback template found for class {classEntity.ClassId} with level ID {classEntity.LevelId}");
                                continue;
                            }

                            var learnersWithFeedback = new List<object>();
                            bool anyLearnersMissingFeedback = false;

                            foreach (var learnerClass in classEntity.Learner_Classes)
                            {
                                // Skip if no valid learner
                                if (learnerClass.LearnerId <= 0 || learnerClass.Learner == null)
                                    continue;

                                var existingFeedback = await _unitOfWork.ClassFeedbackRepository
                                    .GetFeedbackByClassAndLearnerAsync(classEntity.ClassId, learnerClass.LearnerId);

                                if (existingFeedback != null)
                                {
                                    learnersWithFeedback.Add(new
                                    {
                                        LearnerId = learnerClass.LearnerId,
                                        LearnerName = learnerClass.Learner?.FullName ?? "Unknown",
                                        FeedbackId = existingFeedback.FeedbackId,
                                        Status = "Already exists"
                                    });
                                    continue;
                                }

                                // At this point, we know we need to create feedback for this student
                                anyLearnersMissingFeedback = true;

                                var newFeedback = new ClassFeedback
                                {
                                    ClassId = classEntity.ClassId,
                                    LearnerId = learnerClass.LearnerId,
                                    TemplateId = template.TemplateId,
                                    CreatedAt = DateTime.Now,
                                };

                                await _unitOfWork.ClassFeedbackRepository.AddAsync(newFeedback);
                                await _unitOfWork.SaveChangeAsync();

                                if (template.Criteria != null)
                                {
                                    foreach (var criterion in template.Criteria)
                                    {
                                        var evaluation = new ClassFeedbackEvaluation
                                        {
                                            FeedbackId = newFeedback.FeedbackId,
                                            CriterionId = criterion.CriterionId,
                                            AchievedPercentage = 0
                                        };

                                        await _unitOfWork.ClassFeedbackEvaluationRepository.AddAsync(evaluation);
                                    }

                                    await _unitOfWork.SaveChangeAsync();
                                }

                                var notification = new StaffNotification
                                {
                                    LearnerId = learnerClass.LearnerId,
                                    LearningRegisId = null, // As this is a class feedback, not related to learning registration
                                    Type = NotificationType.ClassFeedback,
                                    Status = NotificationStatus.Unread,
                                    CreatedAt = DateTime.Now,
                                    Title = $"Feedback for class '{classEntity.ClassName}'",
                                    Message = $"Please provide your feedback for the class '{classEntity.ClassName}' taught by {classEntity.Teacher.Fullname}."
                                };

                                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                                await _unitOfWork.SaveChangeAsync();

                                feedbacksCreated++;

                                learnersWithFeedback.Add(new
                                {
                                    LearnerId = learnerClass.LearnerId,
                                    LearnerName = learnerClass.Learner?.FullName ?? "Unknown",
                                    FeedbackId = newFeedback.FeedbackId,
                                    Status = "Created"
                                });
                            }

                            if (anyLearnersMissingFeedback || isLastDay)
                            {
                                if (classEntity.TeacherId > 0)
                                {
                                    int newFeedbacksCount = learnersWithFeedback.Count(f => ((dynamic)f).Status == "Created");

                                    if (newFeedbacksCount > 0)
                                    {
                                        // Find any learning registration that links to this teacher
                                        var teacherLearningRegis = await _unitOfWork.LearningRegisRepository
                                            .GetFirstOrDefaultAsync(lr => lr.TeacherId == classEntity.TeacherId);

                                        // If we found a learning registration for this teacher
                                        if (teacherLearningRegis != null)
                                        {
                                            var teacherNotification = new StaffNotification
                                            {
                                                // Use the teacher's learning registration ID to make it appear in their notifications
                                                LearningRegisId = teacherLearningRegis.LearningRegisId,
                                                LearnerId = null,
                                                Type = NotificationType.ClassFeedback,
                                                Status = NotificationStatus.Unread,
                                                CreatedAt = DateTime.Now,
                                                Title = $"Student Feedback Required for Class: {classEntity.ClassId}_{classEntity.ClassName}",
                                                Message = $"Please complete feedback forms for {newFeedbacksCount} students in your {classEntity.Level?.LevelName ?? "N/A"} " +
                                                        $"{classEntity.Major?.MajorName ?? "N/A"} class '{classEntity.ClassName}' (ID: {classEntity.ClassId}). " +
                                                        $"This class has reached its last day. All feedback forms have been prepared with the template '{template.TemplateName}'."
                                            };

                                            await _unitOfWork.StaffNotificationRepository.AddAsync(teacherNotification);
                                            await _unitOfWork.SaveChangeAsync();

                                            _logger.LogInformation($"Created teacher notification for class {classEntity.ClassId} ({classEntity.ClassName})");
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"No learning registration found for teacher ID: {classEntity.TeacherId}, couldn't create notification");
                                        }
                                    }
                                }

                                classesProcessed.Add(new
                                {
                                    ClassId = classEntity.ClassId,
                                    ClassName = classEntity.ClassName,
                                    TeacherId = classEntity.TeacherId,
                                    TeacherName = classEntity.Teacher?.Fullname ?? "N/A",
                                    MajorId = classEntity.MajorId,
                                    MajorName = classEntity.Major?.MajorName ?? "N/A",
                                    LevelId = classEntity.LevelId,
                                    LevelName = classEntity.Level?.LevelName ?? "N/A",
                                    StartDate = classEntity.StartDate,
                                    EndDate = endDate,
                                    Status = classEntity.Status.ToString(),
                                    IsLastDay = isLastDay,
                                    IsRecentlyEnded = isRecentlyEnded,
                                    TemplateId = template.TemplateId,
                                    TemplateName = template.TemplateName,
                                    LearnersCount = classEntity.Learner_Classes.Count,
                                    FeedbacksCreated = learnersWithFeedback.Count(f => ((dynamic)f).Status == "Created"),
                                    LearnerFeedbacks = learnersWithFeedback
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing class {classEntity.ClassId} for feedback");
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Processed {classesProcessed.Count} classes (on last day or recently ended). Created {feedbacksCreated} feedback forms.",
                    Data = classesProcessed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for classes needing feedback");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking for classes needing feedback: {ex.Message}"
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
