using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO;
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

        public FeedbackNotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> CheckLearnerFeedbackNotificationsAsync(int learnerId)
        {
            try
            {
                // Get active learning registrations for this learner
                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearnerId == learnerId &&
                            (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty),
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

                var feedbackNotifications = new List<object>();

                // Get all feedback forms for this learner (both completed and not completed)
                var allFeedbacks = await _unitOfWork.LearningRegisFeedbackRepository
                    .GetFeedbacksByLearnerIdAsync(learnerId);

                foreach (var regis in learningRegs)
                {
                    // Get completed sessions count (attended or absent)
                    var completedSessions = regis.Schedules
                        .Count(s => s.AttendanceStatus == AttendanceStatus.Present ||
                                   s.AttendanceStatus == AttendanceStatus.Absent);

                    // Total number of sessions
                    int totalSessions = regis.NumberOfSession;

                    // Calculate progress percentage
                    double progressPercentage = totalSessions > 0
                        ? (double)completedSessions / totalSessions * 100
                        : 0;

                    // Check if learner has reached the 40% threshold
                    bool reachedThreshold = progressPercentage >= 40;

                    if (!reachedThreshold)
                        continue; // Skip if not reached 40% yet

                    // Find any existing feedback form for this registration
                    var existingFeedback = allFeedbacks
                        .FirstOrDefault(f => f.LearningRegistrationId == regis.LearningRegisId);

                    // If feedback doesn't exist or has NotStarted/InProgress status, include in notifications
                    if (existingFeedback == null)
                    {
                        // Staff hasn't created a feedback form, but learner is eligible for feedback
                        feedbackNotifications.Add(new
                        {
                            LearningRegisId = regis.LearningRegisId,
                            TeacherId = regis.TeacherId,
                            TeacherName = regis.Teacher?.Fullname ?? "N/A",
                            TotalSessions = totalSessions,
                            CompletedSessions = completedSessions,
                            ProgressPercentage = Math.Round(progressPercentage, 2),
                            FeedbackStatus = "NotCreated",
                            Message = $"You have completed {Math.Round(progressPercentage, 2)}% of your learning sessions. Please contact staff to create a feedback form."
                        });
                    }
                    else if (existingFeedback.Status == FeedbackStatus.NotStarted ||
                             existingFeedback.Status == FeedbackStatus.InProgress)
                    {
                        // Staff has created a feedback form, but learner hasn't completed it
                        feedbackNotifications.Add(new
                        {
                            FeedbackId = existingFeedback.FeedbackId,
                            LearningRegisId = regis.LearningRegisId,
                            TeacherId = regis.TeacherId,
                            TeacherName = regis.Teacher?.Fullname ?? "N/A",
                            TotalSessions = totalSessions,
                            CompletedSessions = completedSessions,
                            ProgressPercentage = Math.Round(progressPercentage, 2),
                            FeedbackStatus = existingFeedback.Status.ToString(),
                            CreatedAt = existingFeedback.CreatedAt,
                            Message = $"Please complete the feedback form for your teacher. You have completed {Math.Round(progressPercentage, 2)}% of your learning sessions."
                        });
                    }
                    // Ignore if feedback is already completed
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
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking feedback notifications: {ex.Message}"
                };
            }
        }
    }
}
