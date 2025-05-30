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
                _logger.LogInformation($"Kiểm tra thông báo phản hồi cho ID học viên: {learnerId}");

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
                        Message = "Không tìm thấy đăng ký học tập hoạt động hoặc hồ sơ phản hồi cho học viên này."
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
                            _logger.LogWarning($"Không tìm thấy đăng ký tương ứng với phản hồi đã hoàn tất ID {completedFeedback.FeedbackId}");
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

                    _logger.LogInformation($"                                                                                    ID {completedFeedback.FeedbackId}, registration {regis.LearningRegisId}");
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
                            _logger.LogInformation($"Bỏ qua bản đăng ký {regis.LearningRegisId} vì tỷ lệ hoàn thành dưới 40%");
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
                            existingFeedback.AdditionalComments = "Tự động hoàn thành bởi hệ thống do hết hạn";

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
                        ? "Có thông báo phản hồi."
                        : "Không có thông báo phản hồi vào lúc này.",
                    Data = feedbackNotifications
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feedback notifications for learner {LearnerId}", learnerId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi kiểm tra thông báo phản hồi: {ex.Message}"
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
                        Message = "Không tìm thấy biểu mẫu phản hồi."
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
                        Message = "Không tìm thấy đăng ký học tập."
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
                        Message = "Đã hoàn thành phản hồi. Đăng ký học tập của bạn hiện đã sẵn sàng cho khoản thanh toán 60% còn lại để tiếp tục học.",
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
                        Message = "Đã hoàn thành phản hồi. Đăng ký học tập của bạn đã bị hủy theo yêu cầu.",
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
                    Message = $"Lỗi khi xử lý hoàn thành phản hồi: {ex.Message}"
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
                        Message = "Không tìm thấy học viên có trạng thái thanh toán 40%."
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
                    Message = $"Đã xử lý {learnersWithFortyStatus.Count} đăng ký có trạng thái thanh toán 40%. Đã gửi {notificationsSent} email thông báo.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in automatic feedback notification process");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi trong quá trình thông báo phản hồi tự động: {ex.Message}"
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
                        Message = "Không tìm thấy đăng ký học tập có trạng thái Bốn mươi phần trăm."
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
                    Message = $"Đã xử lý {fortyStatusRegistrations.Count} đăng ký có trạng thái Bốn mươi phần trăm. Đã gửi {notificationCount} thông báo phản hồi.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in learner progress check process");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi trong quá trình kiểm tra tiến độ học viên: {ex.Message}"
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
                        Message = "Không tìm thấy phản hồi đã hết hạn."
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
                                                     "\nTự động hoàn thành bởi hệ thống do hết hạn";

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
                    Message = $"Đã cập nhật {updatedCount} phản hồi đã hết hạn.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for expired feedbacks");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi kiểm tra phản hồi đã hết hạn: {ex.Message}"
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
                        Message = "Không tìm thấy lớp học với loại đăng ký 1002 để tạo phản hồi.",
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
                        Message = "Không tìm thấy lớp học đang hoạt động hoặc mới hoàn thành để tạo phản hồi."
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
                                        Status = "Đã tồn tại"
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
                                    Title = $"Phản hồi cho lớp '{classEntity.ClassName}'",
                                    Message = $"Vui lòng đưa ra phản hồi của bạn cho lớp '{classEntity.ClassName}' được giảng dạy bởi {classEntity.Teacher.Fullname}."
                                };

                                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                                await _unitOfWork.SaveChangeAsync();

                                feedbacksCreated++;

                                learnersWithFeedback.Add(new
                                {
                                    LearnerId = learnerClass.LearnerId,
                                    LearnerName = learnerClass.Learner?.FullName ?? "Unknown",
                                    FeedbackId = newFeedback.FeedbackId,
                                    Status = "Đã tạo"
                                });
                            }

                            if (anyLearnersMissingFeedback || isLastDay)
                            {
                                if (classEntity.TeacherId > 0)
                                {
                                    int newFeedbacksCount = learnersWithFeedback.Count(f => ((dynamic)f).Status == "Đã tạo");

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
                                                Title = $"Yêu cầu phản hồi học viên cho lớp: {classEntity.ClassId}_{classEntity.ClassName}",
                                                Message = $"Vui lòng hoàn thành biểu mẫu phản hồi cho {newFeedbacksCount} học viên trong lớp {classEntity.Level?.LevelName ?? "N/A"} " +
                                                        $"{classEntity.Major?.MajorName ?? "N/A"} '{classEntity.ClassName}' (ID: {classEntity.ClassId}). " +
                                                        $"Lớp học này đã đến ngày cuối cùng. Tất cả các biểu mẫu phản hồi đã được chuẩn bị với mẫu '{template.TemplateName}'."
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
                                    FeedbacksCreated = learnersWithFeedback.Count(f => ((dynamic)f).Status == "Đã tạo"),
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
                    Message = $"Đã xử lý {classesProcessed.Count} lớp học (vào ngày cuối cùng hoặc gần đây đã kết thúc). Đã tạo {feedbacksCreated} biểu mẫu phản hồi.",
                    Data = classesProcessed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for classes needing feedback");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi kiểm tra các lớp học cần phản hồi: {ex.Message}"
                };
            }
        }

        private async Task SendFeedbackEmailNotification(string email, string learnerName, int feedbackId, string teacherName, decimal remainingPayment)
        {
            string subject = "Yêu cầu phản hồi: Tiếp tục hành trình học tập của bạn";

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
