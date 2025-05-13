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

                if (evaluation.LearningRegistration != null)
                {
                    var registration = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(
                            x => x.LearningRegisId == evaluation.LearningRegistrationId,
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
                        dto.TotalSessions = learningRegis.NumberOfSession;

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

        public async Task<ResponseDTO> GetAllQuestionsAsync()
        {
            try
            {
                var questions = await _unitOfWork.TeacherEvaluationRepository.GetAllQuestionsWithOptionsAsync();

                if (questions == null || !questions.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No questions found.",
                        Data = new List<TeacherEvaluationQuestionDTO>()
                    };
                }

                var questionDTOs = _mapper.Map<List<TeacherEvaluationQuestionDTO>>(questions);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {questionDTOs.Count} questions.",
                    Data = questionDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all questions");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving questions: {ex.Message}"
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

        public async Task<ResponseDTO> CreateEvaluationAsync(int learningRegistrationId)
        {
            try
            {
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

                var newEvaluation = new TeacherEvaluationFeedback
                {
                    LearningRegistrationId = learningRegistrationId,
                    TeacherId = registration.TeacherId.Value,
                    LearnerId = registration.LearnerId,
                    CreatedAt = DateTime.Now,
                    Status = TeacherEvaluationStatus.NotStarted,
                    GoalsAchieved = false
                };

                await _unitOfWork.TeacherEvaluationRepository.AddAsync(newEvaluation);
                await _unitOfWork.SaveChangeAsync();

                var savedEvaluation = await _unitOfWork.TeacherEvaluationRepository
                    .GetByLearningRegistrationIdAsync(learningRegistrationId);

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

        public async Task<ResponseDTO> UpdateEvaluationFeedbackAsync(int evaluationFeedbackId, TeacherEvaluationDTO feedbackDTO)
        {
            try
            {
                var existingFeedback = await _unitOfWork.TeacherEvaluationRepository.GetByIdWithDetailsAsync(evaluationFeedbackId);

                if (existingFeedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Evaluation feedback with ID {evaluationFeedbackId} not found."
                    };
                }

                existingFeedback.Status = feedbackDTO.Status;
                existingFeedback.GoalsAchieved = feedbackDTO.GoalsAchieved;

                if (feedbackDTO.Status == TeacherEvaluationStatus.Completed && existingFeedback.CompletedAt == null)
                {
                    existingFeedback.CompletedAt = DateTime.Now;
                }

                await _unitOfWork.TeacherEvaluationRepository.UpdateAsync(existingFeedback);
                await _unitOfWork.SaveChangeAsync();

                var updatedFeedback = await _unitOfWork.TeacherEvaluationRepository.GetByIdWithDetailsAsync(evaluationFeedbackId);
                var updatedFeedbackDTO = _mapper.Map<TeacherEvaluationDTO>(updatedFeedback);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Evaluation feedback updated successfully.",
                    Data = updatedFeedbackDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating evaluation feedback {FeedbackId}", evaluationFeedbackId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error updating evaluation feedback: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeleteEvaluationFeedbackAsync(int evaluationFeedbackId)
        {
            try
            {
                var existingFeedback = await _unitOfWork.TeacherEvaluationRepository.GetByIdWithDetailsAsync(evaluationFeedbackId);

                if (existingFeedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Evaluation feedback with ID {evaluationFeedbackId} not found."
                    };
                }

                await _unitOfWork.TeacherEvaluationRepository.DeleteAsync(evaluationFeedbackId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Evaluation feedback with ID {evaluationFeedbackId} and all its answers have been deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting evaluation feedback {FeedbackId}", evaluationFeedbackId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error deleting evaluation feedback: {ex.Message}"
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
                            OptionText = optionDTO.OptionText,
                        };

                        await _unitOfWork.TeacherEvaluationRepository.AddOptionAsync(option);
                    }

                    await _unitOfWork.SaveChangeAsync();
                }

                var createdQuestion = await _unitOfWork.TeacherEvaluationRepository
                    .GetQuestionWithOptionsAsync(question.EvaluationQuestionId);

                var response = _mapper.Map<TeacherEvaluationQuestionDTO>(createdQuestion);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Question created successfully.",
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

        public async Task<ResponseDTO> SubmitEvaluationAsync(SubmitTeacherEvaluationDTO submitDTO)
        {
            try
            {
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
                        if (regis.TeacherId == null)
                        {
                            continue;
                        }

                        var completedSchedules = regis.Schedules
                            ?.Where(s => s.AttendanceStatus == AttendanceStatus.Present)
                            .OrderByDescending(s => s.StartDay)
                            .ToList();

                        if (completedSchedules == null || !completedSchedules.Any())
                        {
                            continue;
                        }

                        var existingEvaluation = await _unitOfWork.TeacherEvaluationRepository
                            .ExistsByLearningRegistrationIdAsync(regis.LearningRegisId);

                        if (existingEvaluation)
                        {
                            continue;
                        }

                        var newEvaluation = new TeacherEvaluationFeedback
                        {
                            LearningRegistrationId = regis.LearningRegisId,
                            TeacherId = regis.TeacherId.Value,
                            LearnerId = regis.LearnerId,
                            CreatedAt = DateTime.Now,
                            Status = TeacherEvaluationStatus.NotStarted,
                            GoalsAchieved = false
                        };

                        await _unitOfWork.TeacherEvaluationRepository.AddAsync(newEvaluation);
                        await _unitOfWork.SaveChangeAsync();

                        var savedEvaluation = await _unitOfWork.TeacherEvaluationRepository
                            .GetByLearningRegistrationIdAsync(regis.LearningRegisId);

                        requestsCreated++;

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
                throw;
            }
        }
    }
}