using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationOption;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationQuestion;
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

        public async Task<ResponseDTO> GetAllEvaluationsAsync()
        {
            try
            {
                var evaluations = await _unitOfWork.TeacherEvaluationRepository.GetAllEvaluationsWithDetailsAsync();
                var evaluationDTOs = _mapper.Map<List<TeacherEvaluationDTO>>(evaluations);

                foreach (var dto in evaluationDTOs)
                {
                    var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(dto.LearningRegistrationId);
                    if (learningRegis != null)
                    {
                        dto.TotalSessions = learningRegis.NumberOfSession;

                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                        dto.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {evaluationDTOs.Count} evaluations.",
                    Data = evaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all evaluations");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving evaluations: {ex.Message}"
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

                if (evaluation.LearningRegistration != null)
                {
                    var registration = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(
                            x => x.LearningRegisId == learningRegistrationId,
                            "Schedules"
                        );

                    if (registration != null && registration.Any())
                    {
                        var regis = registration.First();

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
                    var evaluationWithDetails = await _unitOfWork.TeacherEvaluationRepository
                        .GetByIdWithDetailsAsync(dto.EvaluationFeedbackId);

                    if (evaluationWithDetails != null && evaluationWithDetails.Answers != null)
                    {
                        dto.Answers = _mapper.Map<List<TeacherEvaluationAnswerDTO>>(evaluationWithDetails.Answers);
                    }

                    var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(dto.LearningRegistrationId);
                    if (learningRegis != null)
                    {
                        dto.TotalSessions = learningRegis.NumberOfSession;

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
                    var evaluationWithDetails = await _unitOfWork.TeacherEvaluationRepository
                        .GetByIdWithDetailsAsync(dto.EvaluationFeedbackId);

                    if (evaluationWithDetails != null && evaluationWithDetails.Answers != null)
                    {
                        dto.Answers = _mapper.Map<List<TeacherEvaluationAnswerDTO>>(evaluationWithDetails.Answers);
                    }

                    var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(dto.LearningRegistrationId);
                    if (learningRegis != null)
                    {
                        dto.TotalSessions = learningRegis.NumberOfSession;

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

        public async Task<ResponseDTO> GetQuestionByIdAsync(int questionId)
        {
            try
            {
                var question = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);

                if (question == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Question with ID {questionId} not found."
                    };
                }

                var questionDTO = _mapper.Map<TeacherEvaluationQuestionDTO>(question);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Question retrieved successfully.",
                    Data = questionDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving question: {ex.Message}"
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

        public async Task<ResponseDTO> ActivateQuestionAsync(int questionId)
        {
            try
            {
                var question = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);

                if (question == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Question with ID {questionId} not found."
                    };
                }

                if (question.IsActive)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Question with ID {questionId} is already active."
                    };
                }

                question.IsActive = true;
                await _unitOfWork.TeacherEvaluationRepository.UpdateQuestionAsync(question);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Question with ID {questionId} has been activated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error activating question: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeactivateQuestionAsync(int questionId)
        {
            try
            {
                var question = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);

                if (question == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Question with ID {questionId} not found."
                    };
                }

                if (!question.IsActive)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Question with ID {questionId} is already inactive."
                    };
                }

                question.IsActive = false;
                await _unitOfWork.TeacherEvaluationRepository.UpdateQuestionAsync(question);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Question with ID {questionId} has been deactivated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error deactivating question: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateQuestionWithOptionsAsync(CreateTeacherEvaluationQuestionDTO questionDTO)
        {
            try
            {
                var question = new TeacherEvaluationQuestion
                {
                    QuestionText = questionDTO.QuestionText,
                    DisplayOrder = questionDTO.DisplayOrder,
                    IsRequired = questionDTO.IsRequired,
                    IsActive = questionDTO.IsActive,
                    Options = new List<TeacherEvaluationOption>()
                };

                await _unitOfWork.TeacherEvaluationRepository.AddQuestionAsync(question);
                await _unitOfWork.SaveChangeAsync();

                if (questionDTO.Options != null && questionDTO.Options.Any())
                {
                    foreach (var optionDTO in questionDTO.Options)
                    {
                        var option = new TeacherEvaluationOption
                        {
                            EvaluationQuestionId = question.EvaluationQuestionId,
                            OptionText = optionDTO.OptionText
                        };

                        await _unitOfWork.TeacherEvaluationRepository.AddOptionAsync(option);
                    }

                    await _unitOfWork.SaveChangeAsync();
                }

                var createdQuestion = await _unitOfWork.TeacherEvaluationRepository
                    .GetQuestionWithOptionsAsync(question.EvaluationQuestionId);

                await CheckAndCreateEvaluationsForLastDayLearners();

                var response = _mapper.Map<TeacherEvaluationQuestionDTO>(createdQuestion);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Question created successfully. Evaluations will appear for teachers when learners reach their last day of one-on-one sessions.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher evaluation question");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error creating question: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateQuestionAsync(int questionId, TeacherEvaluationQuestionDTO questionDTO)
        {
            try
            {
                var existingQuestion = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);

                if (existingQuestion == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Question with ID {questionId} not found."
                    };
                }

                existingQuestion.QuestionText = questionDTO.QuestionText;
                existingQuestion.DisplayOrder = questionDTO.DisplayOrder;
                existingQuestion.IsRequired = questionDTO.IsRequired;
                existingQuestion.IsActive = questionDTO.IsActive;

                _unitOfWork.dbContext.Entry(existingQuestion).State = EntityState.Modified;
                await _unitOfWork.SaveChangeAsync();

                var updatedQuestion = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);
                var updatedQuestionDTO = _mapper.Map<TeacherEvaluationQuestionDTO>(updatedQuestion);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Question updated successfully.",
                    Data = updatedQuestionDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error updating question: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeleteQuestionAsync(int questionId)
        {
            try
            {
                var existingQuestion = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);

                if (existingQuestion == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Question with ID {questionId} not found."
                    };
                }

                var answers = await _unitOfWork.TeacherEvaluationRepository.GetQuery()
                    .Include(f => f.Answers)
                    .SelectMany(f => f.Answers)
                    .Where(a => a.EvaluationQuestionId == questionId)
                    .ToListAsync();

                if (answers != null && answers.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Cannot delete question with ID {questionId} as it is already used in evaluations. Consider setting it as inactive instead."
                    };
                }

                await _unitOfWork.TeacherEvaluationRepository.DeleteAsync(questionId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Question with ID {questionId} and all its options have been deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error deleting question: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> SubmitEvaluationFeedbackAsync(SubmitTeacherEvaluationDTO submitDTO)
        {
            try
            {
                var evaluation = await _unitOfWork.TeacherEvaluationRepository
                    .GetByLearningRegistrationIdAsync(submitDTO.LearningRegistrationId);

                if (evaluation == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Evaluation not found for the specified learning registration."
                    };
                }

                if (evaluation.LearnerId != submitDTO.LearnerId)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "The provided learner ID does not match the evaluation record."
                    };
                }

                evaluation.GoalsAchieved = submitDTO.GoalsAchieved;
                evaluation.Status = TeacherEvaluationStatus.Completed;
                evaluation.CompletedAt = DateTime.Now;

                if (submitDTO.Answers != null && submitDTO.Answers.Count > 0)
                {
                    foreach (var answerDTO in submitDTO.Answers)
                    {
                        var existingAnswer = evaluation.Answers?
                            .FirstOrDefault(a => a.EvaluationQuestionId == answerDTO.EvaluationQuestionId);

                        if (existingAnswer != null)
                        {
                            existingAnswer.SelectedOptionId = answerDTO.SelectedOptionId;

                            await _unitOfWork.TeacherEvaluationRepository.UpdateAsync(evaluation);
                        }
                        else
                        {
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

                await _unitOfWork.TeacherEvaluationRepository.UpdateAsync(evaluation);
                await _unitOfWork.SaveChangeAsync();

                await CreateLearnerEvaluationCompletionNotification(
                    evaluation.LearnerId,
                    evaluation.TeacherId,
                    evaluation.EvaluationFeedbackId,
                    evaluation.LearningRegistrationId
                );

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
                _logger.LogError(ex, "Error submitting evaluation for registration {LearningRegistrationId}", submitDTO.LearningRegistrationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error submitting evaluation: {ex.Message}"
                };
            }
        }

        private async Task CheckAndCreateEvaluationsForLastDayLearners()
        {
            try
            {
                var oneOnOneRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty) &&
                             x.ClassId == null &&
                             x.TeacherId != null,
                        "Learner,Teacher"
                    );

                if (oneOnOneRegistrations == null || !oneOnOneRegistrations.Any())
                {
                    _logger.LogInformation("No active one-on-one learning registrations found for evaluation");
                    return;
                }

                foreach (var registration in oneOnOneRegistrations)
                {
                    if (registration.LearnerId <= 0 || registration.TeacherId <= 0)
                        continue;

                    bool evaluationExists = await _unitOfWork.TeacherEvaluationRepository
                        .ExistsByLearningRegistrationIdAsync(registration.LearningRegisId);

                    if (evaluationExists)
                        continue;

                    var schedules = await _unitOfWork.ScheduleRepository
                        .GetSchedulesByLearningRegisIdAsync(registration.LearningRegisId);

                    if (schedules == null || !schedules.Any())
                        continue;

                    int totalSessions = registration.NumberOfSession;
                    int completedSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                    int remainingSessions = totalSessions - completedSessions;

                    var orderedSchedules = schedules
                        .OrderBy(s => s.StartDay)
                        .ToList();

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var lastScheduleDate = orderedSchedules.LastOrDefault()?.StartDay;

                    bool isLastDay = (lastScheduleDate == today && remainingSessions <= 1);

                    if (!isLastDay)
                        continue;

                    var evaluationFeedback = new TeacherEvaluationFeedback
                    {
                        TeacherId = registration.TeacherId.Value,
                        LearnerId = registration.LearnerId,
                        LearningRegistrationId = registration.LearningRegisId,
                        Status = TeacherEvaluationStatus.NotStarted,
                        CreatedAt = DateTime.Now,
                        GoalsAchieved = false
                    };

                    await _unitOfWork.TeacherEvaluationRepository.AddAsync(evaluationFeedback);
                    await _unitOfWork.SaveChangeAsync();

                    await CreateTeacherEvaluationNotification(
                        registration.TeacherId.Value,
                        evaluationFeedback.EvaluationFeedbackId,
                        registration.LearnerId,
                        registration.LearningRegisId,
                        registration.LearningRequest
                    );

                    _logger.LogInformation(
                        "Created evaluation for teacher {TeacherId} to evaluate learner {LearnerId} on their last day",
                        registration.TeacherId.Value, registration.LearnerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for learners on their last day for evaluation");
                throw;
            }
        }

        private async Task NotifyLearnersAboutNewQuestion(TeacherEvaluationQuestion question)
        {
            try
            {
                // Get all active learning registrations that may need evaluations
                var activeRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty,
                        "Learner"
                    );

                if (activeRegistrations == null || !activeRegistrations.Any())
                {
                    _logger.LogInformation("No active learning registrations found for new question notifications");
                    return;
                }

                foreach (var registration in activeRegistrations)
                {
                    if (registration.LearnerId <= 0)
                        continue;

                    // Check if there's already an evaluation for this registration
                    var existingEvaluation = await _unitOfWork.TeacherEvaluationRepository
                        .ExistsByLearningRegistrationIdAsync(registration.LearningRegisId);

                    if (!existingEvaluation)
                        continue;

                    // Create notification for the learner
                    var notification = new StaffNotification
                    {
                        LearningRegisId = registration.LearningRegisId,
                        LearnerId = registration.LearnerId,
                        Type = NotificationType.Evaluation,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.Now,
                        Title = "New Evaluation Question Added",
                        Message = $"A new question has been added to the teacher evaluation form: '{question.QuestionText}'. " +
                                 $"Please check your evaluation form to provide feedback."
                    };

                    await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                }

                await _unitOfWork.SaveChangeAsync();
                _logger.LogInformation("Notifications sent to learners about new evaluation question: {QuestionId}", question.EvaluationQuestionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating learner notifications for new evaluation question");
                throw;
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
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                string learnerName = learner?.FullName ?? "Learner";

                var notification = new StaffNotification
                {
                    LearningRegisId = learningRegisId,
                    LearnerId = learnerId,
                    Type = NotificationType.Evaluation,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.Now,
                    Title = "Student Evaluation Required",
                    Message = $"Please complete an evaluation for {learnerName}. " +
                             $"Learning goals: {learningGoals}. " +
                             $"Evaluation ID: {evaluationId}"
                };

                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("Created evaluation notification for teacher {TeacherId} regarding learner {LearnerId}",
                    teacherId, learnerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher evaluation notification");
                throw;
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
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                string teacherName = teacher?.Fullname ?? "Your teacher";

                var notification = new StaffNotification
                {
                    LearningRegisId = learningRegisId,
                    LearnerId = learnerId,
                    Type = NotificationType.Evaluation,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.Now,
                    Title = "Teacher Evaluation Form Completed",
                    Message = $"{teacherName} has completed your learning evaluation. " +
                             $"You can now view your evaluation results and feedback. " +
                             $"Evaluation ID: {evaluationId}"
                };

                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("Created evaluation completion notification for learner {LearnerId} from teacher {TeacherId}",
                    learnerId, teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating learner evaluation completion notification");
                throw;
            }
        }

        private async Task MarkTeacherEvaluationNotificationAsResolved(int teacherId, int evaluationId)
        {
            try
            {
                var notifications = await _unitOfWork.StaffNotificationRepository.GetQuery()
                    .Where(n => n.LearningRegistration.TeacherId == teacherId &&
                               n.Type == NotificationType.Evaluation &&
                               (n.Message.Contains($"Evaluation ID: {evaluationId}") ||
                                n.Message.Contains($"ID: {evaluationId}")) &&
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
                    _logger.LogInformation("Marked {0} evaluation notification(s) as resolved for teacher {1}, evaluation {2}",
                        notifications.Count, teacherId, evaluationId);
                }
                else
                {
                    _logger.LogWarning("No evaluation notifications found to mark as resolved for teacher {0}, evaluation {1}",
                        teacherId, evaluationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking teacher evaluation notification as resolved");
                throw;
            }
        }
    }
}