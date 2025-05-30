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
                    Message = $"Đã truy xuất {evaluationDTOs.Count} đánh giá.",
                    Data = evaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all evaluations");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất đánh giá: {ex.Message}"
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
                        Message = $"Không tìm thấy đánh giá cho ID đăng ký học {learningRegistrationId}."
                    };
                }

                var evaluationDTO = _mapper.Map<TeacherEvaluationDTO>(evaluation);

                if (evaluation.Answers != null)
                {
                    evaluationDTO.Answers = _mapper.Map<List<TeacherEvaluationAnswerDTO>>(evaluation.Answers);
                }
                else
                {
                    evaluationDTO.Answers = new List<TeacherEvaluationAnswerDTO>();
                }

                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegistrationId);
                if (learningRegis != null)
                {
                    evaluationDTO.TotalSessions = learningRegis.NumberOfSession;
                    evaluationDTO.LearningRequest = learningRegis.LearningRequest;

                    if (learningRegis.MajorId > 0)
                    {
                        var major = await _unitOfWork.MajorRepository.GetByIdAsync(learningRegis.MajorId);
                        if (major != null)
                        {
                            evaluationDTO.MajorName = major.MajorName;
                        }
                    }

                    var schedules = await _unitOfWork.ScheduleRepository
                        .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                    evaluationDTO.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;

                    if (evaluationDTO.CompletedSessions == evaluationDTO.TotalSessions &&
                        evaluationDTO.Status == TeacherEvaluationStatus.NotStarted)
                    {
                        evaluationDTO.Status = TeacherEvaluationStatus.InProgress;
                        evaluationDTO.Answers = new List<TeacherEvaluationAnswerDTO>();

                        evaluation.Status = TeacherEvaluationStatus.InProgress;
                        _unitOfWork.TeacherEvaluationRepository.UpdateAsync(evaluation);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã truy xuất đánh giá thành công.",
                    Data = evaluationDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluation for learning registration {LearningRegistrationId}", learningRegistrationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất đánh giá: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetEvaluationsByTeacherIdAsync(int teacherId)
        {
            try
            {
                await CheckCompletedSessionsForTeacher(teacherId);

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
                        dto.LearningRequest = learningRegis.LearningRequest;

                        if (learningRegis.MajorId > 0)
                        {
                            var major = await _unitOfWork.MajorRepository.GetByIdAsync(learningRegis.MajorId);
                            if (major != null)
                            {
                                dto.MajorName = major.MajorName;
                            }
                        }

                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                        dto.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;

                        if (dto.CompletedSessions == dto.TotalSessions && dto.Status == TeacherEvaluationStatus.NotStarted)
                        {
                            dto.Status = TeacherEvaluationStatus.InProgress;
                            dto.Answers = new List<TeacherEvaluationAnswerDTO>();
                        }
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã truy xuất {evaluationDTOs.Count} đánh giá cho giáo viên có ID {teacherId}.",
                    Data = evaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluations for teacher {TeacherId}", teacherId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất đánh giá: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetEvaluationsByLearnerIdAsync(int learnerId)
        {
            try
            {
                await CheckCompletedSessionsForLearner(learnerId);

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
                        dto.LearningRequest = learningRegis.LearningRequest;

                        if (learningRegis.MajorId > 0)
                        {
                            var major = await _unitOfWork.MajorRepository.GetByIdAsync(learningRegis.MajorId);
                            if (major != null)
                            {
                                dto.MajorName = major.MajorName;
                            }
                        }

                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                        dto.CompletedSessions = schedules?.Count(s => s.AttendanceStatus == AttendanceStatus.Present) ?? 0;

                        if (dto.CompletedSessions == dto.TotalSessions && dto.Status == TeacherEvaluationStatus.NotStarted)
                        {
                            dto.Status = TeacherEvaluationStatus.InProgress;
                            dto.Answers = new List<TeacherEvaluationAnswerDTO>();
                        }
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã truy xuất {evaluationDTOs.Count} đánh giá cho học viên có ID {learnerId}.",
                    Data = evaluationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evaluations for learner {LearnerId}", learnerId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất đánh giá: {ex.Message}"
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
                        Message = $"Không tìm thấy câu hỏi với ID {questionId}."
                    };
                }

                var questionDTO = _mapper.Map<TeacherEvaluationQuestionDTO>(question);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã truy xuất câu hỏi thành công.",
                    Data = questionDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất câu hỏi: {ex.Message}"
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
                        Message = "Không tìm thấy câu hỏi đánh giá đang hoạt động nào.",
                        Data = new List<TeacherEvaluationQuestionDTO>()
                    };
                }

                var questionDTOs = _mapper.Map<List<TeacherEvaluationQuestionDTO>>(questions);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã truy xuất {questionDTOs.Count} câu hỏi đánh giá đang hoạt động.",
                    Data = questionDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active evaluation questions");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất câu hỏi đánh giá đang hoạt động: {ex.Message}"
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
                        Message = $"Không tìm thấy câu hỏi với ID {questionId}."
                    };
                }

                if (question.IsActive)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Câu hỏi với ID {questionId} đã đang hoạt động."
                    };
                }

                question.IsActive = true;
                await _unitOfWork.TeacherEvaluationRepository.UpdateQuestionAsync(question);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Câu hỏi với ID {questionId} đã được kích hoạt thành công."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi kích hoạt câu hỏi: {ex.Message}"
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
                        Message = $"Không tìm thấy câu hỏi với ID {questionId}."
                    };
                }

                if (!question.IsActive)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Câu hỏi với ID {questionId} đã ở trạng thái không hoạt động."
                    };
                }

                question.IsActive = false;
                await _unitOfWork.TeacherEvaluationRepository.UpdateQuestionAsync(question);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Câu hỏi với ID {questionId} đã được vô hiệu hóa thành công."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi vô hiệu hóa câu hỏi: {ex.Message}"
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
                    Message = "Câu hỏi đã được tạo thành công. Các đánh giá sẽ xuất hiện cho giáo viên khi học viên đến ngày cuối cùng của các buổi học one-on-one.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher evaluation question");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tạo câu hỏi: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateQuestionAsync(int questionId, UpdateTeacherEvaluationQuestionDTO questionDTO)
        {
            try
            {
                var existingQuestion = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);

                if (existingQuestion == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy câu hỏi với ID {questionId}."
                    };
                }

                existingQuestion.QuestionText = questionDTO.QuestionText;
                existingQuestion.DisplayOrder = questionDTO.DisplayOrder;
                existingQuestion.IsRequired = questionDTO.IsRequired;
                existingQuestion.IsActive = questionDTO.IsActive;

                _unitOfWork.dbContext.Entry(existingQuestion).State = EntityState.Modified;
                await _unitOfWork.SaveChangeAsync();

                if (questionDTO.Options != null && questionDTO.Options.Any())
                {
                    var optionIdsInRequest = questionDTO.Options
                        .Where(o => o.EvaluationOptionId > 0)
                        .Select(o => o.EvaluationOptionId)
                        .ToList();

                    var optionsToDelete = existingQuestion.Options
                        .Where(o => !optionIdsInRequest.Contains(o.EvaluationOptionId))
                        .ToList();

                    foreach (var option in optionsToDelete)
                    {
                        _unitOfWork.dbContext.TeacherEvaluationOptions.Remove(option);
                    }

                    if (optionsToDelete.Any())
                    {
                        await _unitOfWork.SaveChangeAsync();
                    }

                    foreach (var optionDTO in questionDTO.Options)
                    {
                        if (optionDTO.EvaluationOptionId > 0)
                        {
                            var existingOption = existingQuestion.Options.FirstOrDefault(o => o.EvaluationOptionId == optionDTO.EvaluationOptionId);
                            if (existingOption != null)
                            {
                                existingOption.OptionText = optionDTO.OptionText;
                                await _unitOfWork.TeacherEvaluationRepository.UpdateOptionAsync(existingOption);
                            }
                        }
                        else
                        {
                            var newOption = new TeacherEvaluationOption
                            {
                                EvaluationQuestionId = questionId,
                                OptionText = optionDTO.OptionText
                            };
                            await _unitOfWork.TeacherEvaluationRepository.AddOptionAsync(newOption);
                        }
                    }

                    await _unitOfWork.SaveChangeAsync();
                }
                else
                {
                    foreach (var option in existingQuestion.Options.ToList())
                    {
                        _unitOfWork.dbContext.TeacherEvaluationOptions.Remove(option);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                var updatedQuestion = await _unitOfWork.TeacherEvaluationRepository.GetQuestionWithOptionsAsync(questionId);
                var updatedQuestionDTO = _mapper.Map<TeacherEvaluationQuestionDTO>(updatedQuestion);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Câu hỏi đã được cập nhật thành công.",
                    Data = updatedQuestionDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật câu hỏi: {ex.Message}"
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
                        Message = $"Không tìm thấy câu hỏi với ID {questionId}."
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
                        Message = $"Không thể xóa câu hỏi với ID {questionId} vì nó đã được sử dụng trong các đánh giá. Hãy xem xét đặt nó thành không hoạt động thay vì xóa."
                    };
                }

                await _unitOfWork.TeacherEvaluationRepository.DeleteAsync(questionId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Câu hỏi với ID {questionId} và tất cả các lựa chọn của nó đã được xóa thành công."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question {QuestionId}", questionId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi xóa câu hỏi: {ex.Message}"
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
                        Message = "Không tìm thấy đánh giá cho đăng ký học đã chỉ định."
                    };
                }

                if (evaluation.LearnerId != submitDTO.LearnerId)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "ID học viên được cung cấp không khớp với hồ sơ đánh giá."
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
                    Message = "Đã gửi đánh giá thành công.",
                    Data = _mapper.Map<TeacherEvaluationDTO>(evaluation)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting evaluation for registration {LearningRegistrationId}", submitDTO.LearningRegistrationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi gửi đánh giá: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CheckForLastDayEvaluationsAsync()
        {
            try
            {
                var evaluationsCreated = await CheckAndCreateEvaluationsForLastDayLearnersWithDetails();

                if (evaluationsCreated.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Đã tạo thành công {evaluationsCreated.Count} đánh giá cho học viên vào ngày cuối cùng của họ",
                        Data = evaluationsCreated
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy học viên nào vào ngày cuối cùng để tạo đánh giá"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for last day evaluations");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi kiểm tra đánh giá ngày cuối cùng: {ex.Message}"
                };
            }
        }

        private async Task<List<object>> CheckAndCreateEvaluationsForLastDayLearnersWithDetails()
        {
            var createdEvaluations = new List<object>();

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
                    return createdEvaluations;
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

                    bool allSessionsCompleted = (completedSessions == totalSessions && totalSessions > 0);

                    if (!allSessionsCompleted)
                    {
                        var orderedSchedules = schedules
                            .OrderBy(s => s.StartDay)
                            .ToList();

                        var today = DateOnly.FromDateTime(DateTime.Today);
                        var lastScheduleDate = orderedSchedules.LastOrDefault()?.StartDay;

                        bool isLastDay = (lastScheduleDate == today && (totalSessions - completedSessions) <= 1);

                        if (!isLastDay)
                            continue;
                    }

                    var evaluationFeedback = new TeacherEvaluationFeedback
                    {
                        TeacherId = registration.TeacherId.Value,
                        LearnerId = registration.LearnerId,
                        LearningRegistrationId = registration.LearningRegisId,
                        Status = allSessionsCompleted ? TeacherEvaluationStatus.InProgress : TeacherEvaluationStatus.NotStarted,
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
                        "Created evaluation for teacher {TeacherId} to evaluate learner {LearnerId} - Sessions: {Completed}/{Total}",
                        registration.TeacherId.Value, registration.LearnerId, completedSessions, totalSessions);

                    createdEvaluations.Add(new
                    {
                        EvaluationId = evaluationFeedback.EvaluationFeedbackId,
                        TeacherId = registration.TeacherId.Value,
                        TeacherName = registration.Teacher?.Fullname ?? "Unknown",
                        LearnerId = registration.LearnerId,
                        LearnerName = registration.Learner?.FullName ?? "Unknown",
                        LearningRegistrationId = registration.LearningRegisId,
                        TotalSessions = totalSessions,
                        CompletedSessions = completedSessions,
                        Status = evaluationFeedback.Status.ToString(),
                        CreationReason = allSessionsCompleted ? "All sessions completed" : "Last day of sessions",
                        LearningRequest = registration.LearningRequest
                    });
                }

                return createdEvaluations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for completed sessions for evaluation");
                throw;
            }
        }

        private async Task CheckAndCreateEvaluationsForLastDayLearners()
        {
            await CheckAndCreateEvaluationsForLastDayLearnersWithDetails();
        }

        private async Task CheckCompletedSessionsForTeacher(int teacherId)
        {
            try
            {
                var registrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty) &&
                             x.ClassId == null &&
                             x.TeacherId == teacherId,
                        "Learner,Teacher"
                    );

                if (registrations == null || !registrations.Any())
                    return;

                foreach (var registration in registrations)
                {
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

                    if (completedSessions == totalSessions && totalSessions > 0)
                    {
                        var evaluationFeedback = new TeacherEvaluationFeedback
                        {
                            TeacherId = teacherId,
                            LearnerId = registration.LearnerId,
                            LearningRegistrationId = registration.LearningRegisId,
                            Status = TeacherEvaluationStatus.InProgress,
                            CreatedAt = DateTime.Now,
                            GoalsAchieved = false
                        };

                        await _unitOfWork.TeacherEvaluationRepository.AddAsync(evaluationFeedback);
                        await _unitOfWork.SaveChangeAsync();

                        _logger.LogInformation(
                            "Created evaluation for teacher {TeacherId} to evaluate learner {LearnerId} - All sessions completed ({Completed}/{Total})",
                            teacherId, registration.LearnerId, completedSessions, totalSessions);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking completed sessions for teacher {TeacherId}", teacherId);
            }
        }

        private async Task CheckCompletedSessionsForLearner(int learnerId)
        {
            try
            {
                var registrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty) &&
                             x.ClassId == null &&
                             x.LearnerId == learnerId,
                        "Learner,Teacher"
                    );

                if (registrations == null || !registrations.Any())
                    return;

                foreach (var registration in registrations)
                {
                    if (registration.TeacherId <= 0)
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

                    if (completedSessions == totalSessions && totalSessions > 0)
                    {
                        var evaluationFeedback = new TeacherEvaluationFeedback
                        {
                            TeacherId = registration.TeacherId.Value,
                            LearnerId = learnerId,
                            LearningRegistrationId = registration.LearningRegisId,
                            Status = TeacherEvaluationStatus.InProgress,
                            CreatedAt = DateTime.Now,
                            GoalsAchieved = false
                        };

                        await _unitOfWork.TeacherEvaluationRepository.AddAsync(evaluationFeedback);
                        await _unitOfWork.SaveChangeAsync();

                        _logger.LogInformation(
                            "Created evaluation for teacher {TeacherId} to evaluate learner {LearnerId} - All sessions completed ({Completed}/{Total})",
                            registration.TeacherId.Value, learnerId, completedSessions, totalSessions);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking completed sessions for learner {LearnerId}", learnerId);
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
                    Title = "Yêu cầu đánh giá học viên",
                    Message = $"Vui lòng hoàn thành đánh giá cho {learnerName}. " +
                             $"Mục tiêu học tập: {learningGoals}. " +
                             $"ID đánh giá: {evaluationId}"
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
                    Title = "Giáo viên đã hoàn thành mẫu đánh giá",
                    Message = $"{teacherName} đã hoàn thành đánh giá học tập của bạn. " +
                             $"Bạn có thể xem kết quả đánh giá và phản hồi ngay bây giờ. " +
                             $"ID đánh giá: {evaluationId}"
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