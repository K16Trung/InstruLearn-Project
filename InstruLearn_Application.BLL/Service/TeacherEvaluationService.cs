// InstruLearn_Application.BLL/Service/TeacherEvaluationService.cs
using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class TeacherEvaluationService : ITeacherEvaluationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherEvaluationService> _logger;

        public TeacherEvaluationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TeacherEvaluationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetEvaluationByIdAsync(int evaluationFeedbackId)
        {
            try
            {
                var evaluation = await _unitOfWork.TeacherEvaluationRepository.GetByIdWithDetailsAsync(evaluationFeedbackId);

                if (evaluation == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Evaluation with ID {evaluationFeedbackId} not found."
                    };
                }

                var evaluationDTO = _mapper.Map<TeacherEvaluationDTO>(evaluation);

                // Add extra details from learning registration
                if (evaluation.LearningRegistration != null)
                {
                    evaluationDTO.InitialLearningRequest = evaluation.LearningRegistration.LearningRequest;

                    // Get learning registration with schedules
                    var registration = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(
                            x => x.LearningRegisId == evaluation.LearningRegistrationId,
                            "Schedules"
                        );

                    if (registration != null && registration.Any())
                    {
                        var regis = registration.First();

                        // Calculate completed sessions
                        var completedSessions = regis.Schedules
                            .Count(s => s.AttendanceStatus == AttendanceStatus.Present);

                        evaluationDTO.CompletedSessions = completedSessions;
                        evaluationDTO.TotalSessions = regis.NumberOfSession;
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Evaluation retrieved successfully.",
                    Data = evaluationDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluation {EvaluationId}", evaluationFeedbackId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving evaluation: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetEvaluationByRegistrationIdAsync(int learningRegistrationId)
        {
            try
            {
                var evaluation = await _unitOfWork.TeacherEvaluationRepository.GetByLearningRegistrationIdAsync(learningRegistrationId);

                if (evaluation == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"No evaluation found for learning registration ID {learningRegistrationId}."
                    };
                }

                var evaluationDTO = _mapper.Map<TeacherEvaluationDTO>(evaluation);

                // Add extra details from learning registration
                if (evaluation.LearningRegistration != null)
                {
                    evaluationDTO.InitialLearningRequest = evaluation.LearningRegistration.LearningRequest;

                    // Get learning registration with schedules
                    var registration = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(
                            x => x.LearningRegisId == learningRegistrationId,
                            "Schedules"
                        );

                    if (registration != null && registration.Any())
                    {
                        var regis = registration.First();

                        // Calculate completed sessions
                        var completedSessions = regis.Schedules
                            .Count(s => s.AttendanceStatus == AttendanceStatus.Present);

                        evaluationDTO.CompletedSessions = completedSessions;
                        evaluationDTO.TotalSessions = regis.NumberOfSession;
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Evaluation retrieved successfully.",
                    Data = evaluationDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluation for registration {LearningRegistrationId}", learningRegistrationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving evaluation: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetEvaluationsByTeacherIdAsync(int teacherId)
        {
            try
            {
                var evaluations = await _unitOfWork.TeacherEvaluationRepository.GetByTeacherIdAsync(teacherId);
                var evaluationDTOs = _mapper.Map<List<TeacherEvaluationDTO>>(evaluations);

                foreach (var dto in evaluationDTOs)
                {
                    var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(dto.LearningRegistrationId);
                    if (learningRegis != null)
                    {
                        dto.InitialLearningRequest = learningRegis.LearningRequest;
                        dto.TotalSessions = learningRegis.NumberOfSession;

                        // Get schedules for the registration
                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                        dto.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {evaluationDTOs.Count} evaluations for teacher ID {teacherId}.",
                    Data = evaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluations for teacher {TeacherId}", teacherId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving evaluations: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetEvaluationsByLearnerIdAsync(int learnerId)
        {
            try
            {
                var evaluations = await _unitOfWork.TeacherEvaluationRepository.GetByLearnerIdAsync(learnerId);
                var evaluationDTOs = _mapper.Map<List<TeacherEvaluationDTO>>(evaluations);

                foreach (var dto in evaluationDTOs)
                {
                    var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(dto.LearningRegistrationId);
                    if (learningRegis != null)
                    {
                        dto.InitialLearningRequest = learningRegis.LearningRequest;
                        dto.TotalSessions = learningRegis.NumberOfSession;

                        // Get schedules for the registration
                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                        dto.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {evaluationDTOs.Count} evaluations for learner ID {learnerId}.",
                    Data = evaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluations for learner {LearnerId}", learnerId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving evaluations: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetPendingEvaluationsForTeacherAsync(int teacherId)
        {
            try
            {
                var pendingEvaluations = await _unitOfWork.TeacherEvaluationRepository.GetPendingByTeacherIdAsync(teacherId);
                var pendingEvaluationDTOs = _mapper.Map<List<TeacherEvaluationDTO>>(pendingEvaluations);

                foreach (var dto in pendingEvaluationDTOs)
                {
                    var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(dto.LearningRegistrationId);
                    if (learningRegis != null)
                    {
                        dto.InitialLearningRequest = learningRegis.LearningRequest;
                        dto.TotalSessions = learningRegis.NumberOfSession;

                        // Get schedules for the registration
                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                        dto.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {pendingEvaluationDTOs.Count} pending evaluations for teacher ID {teacherId}.",
                    Data = pendingEvaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending evaluations for teacher {TeacherId}", teacherId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving pending evaluations: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateEvaluationAsync(int learningRegistrationId)
        {
            try
            {
                // Check if an evaluation already exists for this registration
                var existingEvaluation = await _unitOfWork.TeacherEvaluationRepository
                    .ExistsByLearningRegistrationIdAsync(learningRegistrationId);

                if (existingEvaluation)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "An evaluation already exists for this learning registration."
                    };
                }

                // Get the learning registration details
                var learningRegis = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearningRegisId == learningRegistrationId,
                        "Teacher,Learner"
                    );

                if (learningRegis == null || !learningRegis.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning registration not found."
                    };
                }

                var registration = learningRegis.First();

                if (registration.TeacherId == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "The learning registration does not have an assigned teacher."
                    };
                }

                // Create a new evaluation record
                var newEvaluation = new TeacherEvaluationFeedback
                {
                    LearningRegistrationId = learningRegistrationId,
                    TeacherId = registration.TeacherId.Value,
                    LearnerId = registration.LearnerId,
                    CreatedAt = DateTime.Now,
                    Status = TeacherEvaluationStatus.NotStarted,
                    GoalsAssessment = "",
                    ProgressRating = 0,
                    GoalsAchieved = false
                };

                await _unitOfWork.TeacherEvaluationRepository.AddAsync(newEvaluation);
                await _unitOfWork.SaveChangeAsync();

                // Get the newly created evaluation with its ID
                var savedEvaluation = await _unitOfWork.TeacherEvaluationRepository
                    .GetByLearningRegistrationIdAsync(learningRegistrationId);

                // Create a system notification for the teacher
                await CreateTeacherEvaluationNotification(
                    registration.TeacherId.Value,
                    savedEvaluation.EvaluationFeedbackId,
                    registration.LearnerId,
                    learningRegistrationId,
                    registration.LearningRequest
                );

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Evaluation created successfully. Teacher has been notified.",
                    Data = _mapper.Map<TeacherEvaluationDTO>(savedEvaluation)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating evaluation for learning registration {LearningRegistrationId}", learningRegistrationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error creating evaluation: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> SubmitEvaluationAsync(SubmitTeacherEvaluationDTO submitDTO)
        {
            try
            {
                // Get the existing evaluation
                var evaluation = await _unitOfWork.TeacherEvaluationRepository
                    .GetByIdWithDetailsAsync(submitDTO.EvaluationFeedbackId);

                if (evaluation == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Evaluation not found."
                    };
                }

                // Update the evaluation fields
                evaluation.GoalsAssessment = submitDTO.GoalsAssessment;
                evaluation.ProgressRating = submitDTO.ProgressRating;
                evaluation.GoalsAchieved = submitDTO.GoalsAchieved;
                evaluation.Status = TeacherEvaluationStatus.Completed;
                evaluation.CompletedAt = DateTime.Now;

                // Process answers
                if (submitDTO.Answers != null && submitDTO.Answers.Count > 0)
                {
                    foreach (var answerDTO in submitDTO.Answers)
                    {
                        // Check if an answer already exists for this question
                        var existingAnswer = evaluation.Answers?
                            .FirstOrDefault(a => a.EvaluationQuestionId == answerDTO.EvaluationQuestionId);

                        if (existingAnswer != null)
                        {
                            // Update existing answer
                            existingAnswer.SelectedOptionId = answerDTO.SelectedOptionId;

                            // Since UpdateAnswerAsync doesn't exist in the interface, use a generic update method
                            await _unitOfWork.TeacherEvaluationRepository.UpdateAsync(evaluation);
                        }
                        else
                        {
                            // Create new answer
                            var newAnswer = new TeacherEvaluationAnswer
                            {
                                EvaluationFeedbackId = evaluation.EvaluationFeedbackId,
                                EvaluationQuestionId = answerDTO.EvaluationQuestionId,
                                SelectedOptionId = answerDTO.SelectedOptionId
                            };

                            await _unitOfWork.TeacherEvaluationRepository.AddAnswerAsync(newAnswer);
                        }
                    }
                }

                // Save all changes
                await _unitOfWork.TeacherEvaluationRepository.UpdateAsync(evaluation);
                await _unitOfWork.SaveChangeAsync();

                // Create a notification for the learner that their evaluation is complete
                await CreateLearnerEvaluationCompletionNotification(
                    evaluation.LearnerId,
                    evaluation.TeacherId,
                    evaluation.EvaluationFeedbackId,
                    evaluation.LearningRegistrationId
                );

                // Mark the teacher's notification as resolved
                await MarkTeacherEvaluationNotificationAsResolved(evaluation.TeacherId, evaluation.EvaluationFeedbackId);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Evaluation submitted successfully.",
                    Data = _mapper.Map<TeacherEvaluationDTO>(evaluation)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting evaluation {EvaluationId}", submitDTO.EvaluationFeedbackId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error submitting evaluation: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CheckAndCreateEvaluationRequestsAsync()
        {
            try
            {
                _logger.LogInformation("Starting automatic check for teacher evaluation requests");

                // Get active learning registrations
                // Since LearningRegis.Hundred doesn't exist, using LearningRegis.Sixty instead which is likely similar
                var activeRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty,
                        "Teacher,Learner,Schedules"
                    );

                if (activeRegistrations == null || !activeRegistrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No active learning registrations found."
                    };
                }

                int requestsCreated = 0;
                var results = new List<object>();

                foreach (var regis in activeRegistrations)
                {
                    try
                    {
                        // Skip if no teacher assigned
                        if (regis.TeacherId == null)
                        {
                            continue;
                        }

                        // Get schedules where attendance was marked as "Present"
                        var completedSchedules = regis.Schedules
                            ?.Where(s => s.AttendanceStatus == AttendanceStatus.Present)
                            .OrderByDescending(s => s.StartDay)
                            .ToList();

                        if (completedSchedules == null || !completedSchedules.Any())
                        {
                            continue; // Skip if no schedules with "Present" attendance
                        }

                        // Check if we already have an evaluation for this registration
                        var existingEvaluation = await _unitOfWork.TeacherEvaluationRepository
                            .ExistsByLearningRegistrationIdAsync(regis.LearningRegisId);

                        if (existingEvaluation)
                        {
                            continue; // Skip if evaluation already exists
                        }

                        // Create a new teacher evaluation request
                        var newEvaluation = new TeacherEvaluationFeedback
                        {
                            LearningRegistrationId = regis.LearningRegisId,
                            TeacherId = regis.TeacherId.Value,
                            LearnerId = regis.LearnerId,
                            CreatedAt = DateTime.Now,
                            Status = TeacherEvaluationStatus.NotStarted,
                            GoalsAssessment = "",
                            ProgressRating = 0,
                            GoalsAchieved = false
                        };

                        await _unitOfWork.TeacherEvaluationRepository.AddAsync(newEvaluation);
                        await _unitOfWork.SaveChangeAsync();

                        // Get the newly created evaluation with its ID
                        var savedEvaluation = await _unitOfWork.TeacherEvaluationRepository
                            .GetByLearningRegistrationIdAsync(regis.LearningRegisId);

                        requestsCreated++;

                        // Create a system notification for the teacher
                        await CreateTeacherEvaluationNotification(
                            regis.TeacherId.Value,
                            savedEvaluation.EvaluationFeedbackId,
                            regis.LearnerId,
                            regis.LearningRegisId,
                            regis.LearningRequest
                        );

                        results.Add(new
                        {
                            LearningRegisId = regis.LearningRegisId,
                            TeacherId = regis.TeacherId,
                            TeacherName = regis.Teacher?.Fullname ?? "N/A",
                            LearnerId = regis.LearnerId,
                            LearnerName = regis.Learner?.FullName ?? "N/A",
                            EvaluationId = savedEvaluation.EvaluationFeedbackId,
                            CreatedAt = savedEvaluation.CreatedAt,
                            NotificationCreated = true
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing evaluation request for registration {LearningRegisId}",
                            regis.LearningRegisId);

                        results.Add(new
                        {
                            LearningRegisId = regis.LearningRegisId,
                            Error = ex.Message,
                            Success = false
                        });
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Created {requestsCreated} teacher evaluation requests.",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and creating teacher evaluation requests");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error creating teacher evaluation requests: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetActiveQuestionsAsync()
        {
            try
            {
                var questions = await _unitOfWork.TeacherEvaluationRepository.GetActiveQuestionsWithOptionsAsync();

                if (questions == null || !questions.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No active evaluation questions found.",
                        Data = new List<TeacherEvaluationQuestionDTO>()
                    };
                }

                var questionDTOs = _mapper.Map<List<TeacherEvaluationQuestionDTO>>(questions);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {questionDTOs.Count} active evaluation questions.",
                    Data = questionDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active evaluation questions");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving active evaluation questions: {ex.Message}"
                };
            }
        }

        private async Task CreateTeacherEvaluationNotification(
            int teacherId,
            int evaluationId,
            int learnerId,
            int learningRegisId,
            string learningGoals)
        {
            try
            {
                // Get learner name for the notification
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                string learnerName = learner?.FullName ?? "Learner";

                // Create a notification for the teacher
                var notification = new StaffNotification
                {
                    LearningRegisId = learningRegisId,
                    LearnerId = learnerId,
                    Type = NotificationType.Evaluation,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.Now,
                    Title = "Student Evaluation Required",
                    Message = $"Please complete an evaluation for {learnerName} (ID: {evaluationId}). " +
                             $"Learning goals: {learningGoals}"
                };

                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation($"Created evaluation notification for teacher {teacherId} regarding learner {learnerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher evaluation notification");
                throw; // Rethrow to allow the calling method to handle it
            }
        }

        private async Task CreateLearnerEvaluationCompletionNotification(
            int learnerId,
            int teacherId,
            int evaluationId,
            int learningRegisId)
        {
            try
            {
                // Get teacher name for the notification
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                string teacherName = teacher?.Fullname ?? "Your teacher";

                // Create a notification for the learner
                var notification = new StaffNotification
                {
                    LearningRegisId = learningRegisId,
                    LearnerId = learnerId,
                    Type = NotificationType.Evaluation,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.Now,
                    Title = "Learning Evaluation Completed",
                    Message = $"{teacherName} has completed your learning evaluation (ID: {evaluationId}). " +
                             $"View your evaluation results and feedback."
                };

                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation($"Created evaluation completion notification for learner {learnerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating learner evaluation completion notification");
                throw; // Rethrow to allow the calling method to handle it
            }
        }

        private async Task MarkTeacherEvaluationNotificationAsResolved(int teacherId, int evaluationId)
        {
            try
            {
                // Find the notification(s) for this evaluation by searching in the message content
                var notifications = await _unitOfWork.StaffNotificationRepository.GetQuery()
                    .Where(n => n.LearningRegistration.TeacherId == teacherId &&
                               n.Type == NotificationType.Evaluation &&
                               n.Message.Contains($"ID: {evaluationId}") &&
                               n.Status != NotificationStatus.Resolved)
                    .ToListAsync();

                if (notifications != null && notifications.Any())
                {
                    foreach (var notification in notifications)
                    {
                        notification.Status = NotificationStatus.Resolved;
                        await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                    }

                    await _unitOfWork.SaveChangeAsync();
                    _logger.LogInformation($"Marked evaluation notification(s) as resolved for teacher {teacherId}, evaluation {evaluationId}");
                }
                else
                {
                    _logger.LogWarning($"No evaluation notifications found to mark as resolved for teacher {teacherId}, evaluation {evaluationId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking teacher evaluation notification as resolved");
                throw; // Rethrow to allow the calling method to handle it
            }
        }
    }
}