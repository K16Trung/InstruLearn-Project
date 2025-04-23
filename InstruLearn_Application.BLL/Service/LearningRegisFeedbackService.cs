using AutoMapper;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.FeedbackSummary;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackOption;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackQuestion;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.BLL.Service.IService;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisFeedbackService : ILearningRegisFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LearningRegisFeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> CreateQuestionAsync(LearningRegisFeedbackQuestionDTO questionDTO)
        {
            var question = new LearningRegisFeedbackQuestion
            {
                QuestionText = questionDTO.QuestionText,
                DisplayOrder = questionDTO.DisplayOrder,
                IsRequired = questionDTO.IsRequired,
                IsActive = true,
                Options = new List<LearningRegisFeedbackOption>()
            };

            await _unitOfWork.LearningRegisFeedbackQuestionRepository.AddAsync(question);
            await _unitOfWork.SaveChangeAsync();

            if (questionDTO.Options != null)
            {
                foreach (var optionDTO in questionDTO.Options)
                {
                    var option = new LearningRegisFeedbackOption
                    {
                        QuestionId = question.QuestionId,
                        OptionText = optionDTO.OptionText
                    };
                    await _unitOfWork.LearningRegisFeedbackOptionRepository.AddAsync(option);
                }
                await _unitOfWork.SaveChangeAsync();
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Tạo câu hỏi đánh giá thành công"
            };
        }


        public async Task<ResponseDTO> UpdateQuestionAsync(int questionId, LearningRegisFeedbackQuestionDTO questionDTO)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetQuestionWithOptionsAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            question.QuestionText = questionDTO.QuestionText;
            question.DisplayOrder = questionDTO.DisplayOrder;
            question.IsRequired = questionDTO.IsRequired;

            await _unitOfWork.LearningRegisFeedbackQuestionRepository.UpdateAsync(question);

            // Update options
            if (questionDTO.Options != null)
            {
                var existingOptions = await _unitOfWork.LearningRegisFeedbackOptionRepository.GetOptionsByQuestionIdAsync(questionId);
                var existingOptionIds = existingOptions.Select(o => o.OptionId).ToList();
                var updatedOptionIds = questionDTO.Options.Where(o => o.OptionId > 0).Select(o => o.OptionId).ToList();

                // Remove options that are not in the updated list
                foreach (var optionId in existingOptionIds)
                {
                    if (!updatedOptionIds.Contains(optionId))
                    {
                        await _unitOfWork.LearningRegisFeedbackOptionRepository.DeleteAsync(optionId);
                    }
                }

                // Update or add options
                foreach (var optionDTO in questionDTO.Options)
                {
                    if (optionDTO.OptionId > 0)
                    {
                        // Update existing option
                        var option = existingOptions.FirstOrDefault(o => o.OptionId == optionDTO.OptionId);
                        if (option != null)
                        {
                            option.OptionText = optionDTO.OptionText;
                            await _unitOfWork.LearningRegisFeedbackOptionRepository.UpdateAsync(option);
                        }
                    }
                    else
                    {
                        // Add new option
                        var newOption = new LearningRegisFeedbackOption
                        {
                            QuestionId = questionId,
                            OptionText = optionDTO.OptionText,
                        };
                        await _unitOfWork.LearningRegisFeedbackOptionRepository.AddAsync(newOption);
                    }
                }
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Cập nhật câu hỏi đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> DeleteQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            // Delete options first
            var options = await _unitOfWork.LearningRegisFeedbackOptionRepository.GetOptionsByQuestionIdAsync(questionId);
            foreach (var option in options)
            {
                await _unitOfWork.LearningRegisFeedbackOptionRepository.DeleteAsync(option.OptionId);
            }

            await _unitOfWork.LearningRegisFeedbackQuestionRepository.DeleteAsync(questionId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Xóa câu hỏi đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> ActivateQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            question.IsActive = true;
            await _unitOfWork.LearningRegisFeedbackQuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Kích hoạt câu hỏi thành công"
            };
        }

        public async Task<ResponseDTO> DeactivateQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            question.IsActive = false;
            await _unitOfWork.LearningRegisFeedbackQuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Vô hiệu hóa câu hỏi thành công"
            };
        }

        public async Task<LearningRegisFeedbackQuestionDTO> GetQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetQuestionWithOptionsAsync(questionId);
            if (question == null)
                return null;

            return MapToQuestionDTO(question);
        }

        public async Task<List<LearningRegisFeedbackQuestionDTO>> GetAllActiveQuestionsAsync()
        {
            var questions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetActiveQuestionsWithOptionsAsync();
            return questions.Select(MapToQuestionDTO).ToList();
        }

        public async Task<ResponseDTO> SubmitFeedbackAsync(CreateLearningRegisFeedbackDTO createDTO)
        {
            // Validate learning registration
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(createDTO.LearningRegistrationId);
            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy đăng ký học"
                };
            }

            // Validate learner
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(createDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy học viên"
                };
            }

            // Check if feedback already exists
            var existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackByRegistrationIdAsync(createDTO.LearningRegistrationId);
            if (existingFeedback != null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Đã tồn tại đánh giá cho đăng ký học này"
                };
            }

            // Get all active questions to validate
            var activeQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetActiveQuestionsWithOptionsAsync();
            if (activeQuestions == null || !activeQuestions.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi đánh giá nào đang hoạt động"
                };
            }

            var requiredQuestionIds = activeQuestions.Where(q => q.IsRequired).Select(q => q.QuestionId).ToList();
            var answeredQuestionIds = createDTO.Answers.Select(a => a.QuestionId).ToList();

            // Check if all required questions are answered
            var missingRequiredQuestions = requiredQuestionIds.Except(answeredQuestionIds).ToList();
            if (missingRequiredQuestions.Any())
            {
                var missingQuestionText = string.Join(", ", activeQuestions
                    .Where(q => missingRequiredQuestions.Contains(q.QuestionId))
                    .Select(q => q.QuestionText));

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Vui lòng trả lời các câu hỏi bắt buộc: {missingQuestionText}"
                };
            }

            // Create feedback
            var feedback = new LearningRegisFeedback
            {
                LearningRegistrationId = createDTO.LearningRegistrationId,
                LearnerId = createDTO.LearnerId,
                AdditionalComments = createDTO.AdditionalComments,
                CreatedAt = DateTime.Now,
                CompletedAt = DateTime.Now,
                Status = FeedbackStatus.Completed,
                Answers = new List<LearningRegisFeedbackAnswer>()
            };

            // Add answers
            foreach (var answerDTO in createDTO.Answers)
            {
                // Validate question exists and is active
                var question = activeQuestions.FirstOrDefault(q => q.QuestionId == answerDTO.QuestionId);
                if (question == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Câu hỏi không hợp lệ (ID: {answerDTO.QuestionId})"
                    };
                }

                // Validate option belongs to the question
                if (question.Options == null || !question.Options.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Câu hỏi (ID: {answerDTO.QuestionId}) không có lựa chọn nào"
                    };
                }

                // Validate option belongs to the question
                var option = question.Options.FirstOrDefault(o => o.OptionId == answerDTO.SelectedOptionId);
                if (option == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Lựa chọn không hợp lệ cho câu hỏi (ID: {answerDTO.QuestionId})"
                    };
                }

                feedback.Answers.Add(new LearningRegisFeedbackAnswer
                {
                    QuestionId = answerDTO.QuestionId,
                    SelectedOptionId = answerDTO.SelectedOptionId,
                });
            }

            // Save to database
            try
            {
                await _unitOfWork.LearningRegisFeedbackRepository.AddAsync(feedback);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Gửi đánh giá thành công"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lưu đánh giá: {ex.Message}"
                };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Gửi đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateLearningRegisFeedbackDTO updateDTO)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackWithDetailsAsync(feedbackId);
            if (feedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy đánh giá"
                };
            }

            // Update additional comments
            feedback.AdditionalComments = updateDTO.AdditionalComments;
            feedback.CompletedAt = DateTime.Now;

            // Get active questions for validation
            var activeQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetActiveQuestionsWithOptionsAsync();

            // Delete existing answers
            var existingAnswers = await _unitOfWork.LearningRegisFeedbackAnswerRepository.GetAnswersByFeedbackIdAsync(feedbackId);
            foreach (var answer in existingAnswers)
            {
                await _unitOfWork.LearningRegisFeedbackAnswerRepository.DeleteAsync(answer.AnswerId);
            }

            // Add new answers
            foreach (var answerDTO in updateDTO.Answers)
            {
                // Validate question exists and is active
                var question = activeQuestions.FirstOrDefault(q => q.QuestionId == answerDTO.QuestionId);
                if (question == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Câu hỏi không hợp lệ (ID: {answerDTO.QuestionId})"
                    };
                }

                // Validate option belongs to the question
                var option = question.Options.FirstOrDefault(o => o.OptionId == answerDTO.SelectedOptionId);
                if (option == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Lựa chọn không hợp lệ cho câu hỏi (ID: {answerDTO.QuestionId})"
                    };
                }

                var newAnswer = new LearningRegisFeedbackAnswer
                {
                    FeedbackId = feedbackId,
                    QuestionId = answerDTO.QuestionId,
                    SelectedOptionId = answerDTO.SelectedOptionId,
                };

                await _unitOfWork.LearningRegisFeedbackAnswerRepository.AddAsync(newAnswer);
            }

            await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Cập nhật đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy đánh giá"
                };
            }

            // Delete answers first
            var answers = await _unitOfWork.LearningRegisFeedbackAnswerRepository.GetAnswersByFeedbackIdAsync(feedbackId);
            foreach (var answer in answers)
            {
                await _unitOfWork.LearningRegisFeedbackAnswerRepository.DeleteAsync(answer.AnswerId);
            }

            await _unitOfWork.LearningRegisFeedbackRepository.DeleteAsync(feedbackId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Xóa đánh giá thành công"
            };
        }

        public async Task<LearningRegisFeedbackDTO> GetFeedbackByIdAsync(int feedbackId)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackWithDetailsAsync(feedbackId);
            if (feedback == null)
                return null;

            return MapToFeedbackDTO(feedback);
        }

        public async Task<LearningRegisFeedbackDTO> GetFeedbackByRegistrationIdAsync(int registrationId)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackByRegistrationIdAsync(registrationId);
            if (feedback == null)
                return null;

            return MapToFeedbackDTO(feedback);
        }

        public async Task<List<LearningRegisFeedbackDTO>> GetFeedbacksByTeacherIdAsync(int teacherId)
        {
            var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByTeacherIdAsync(teacherId);
            return feedbacks.Select(MapToFeedbackDTO).ToList();
        }

        public async Task<List<LearningRegisFeedbackDTO>> GetFeedbacksByLearnerIdAsync(int learnerId)
        {
            var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByLearnerIdAsync(learnerId);
            return feedbacks.Select(MapToFeedbackDTO).ToList();
        }

        public async Task<TeacherFeedbackSummaryDTO> GetTeacherFeedbackSummaryAsync(int teacherId)
        {
            var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByTeacherIdAsync(teacherId);
            if (feedbacks == null || !feedbacks.Any())
            {
                var teachers = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                return new TeacherFeedbackSummaryDTO
                {
                    TeacherId = teacherId,
                    TeacherName = teachers?.Fullname ?? "Unknown",
                    TotalFeedbacks = 0,
                    OverallAverageRating = 0,
                    CategoryAverages = new Dictionary<string, double>(),
                    QuestionSummaries = new List<QuestionSummaryDTO>()
                };
            }

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);

            // Get all questions and options for reference
            var allQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetAllAsync();

            // Track overall statistics
            double overallRatingSum = 0;
            int overallRatingCount = 0;

            // Track category statistics - use a constant category since we don't have Category property
            var categoryRatings = new Dictionary<string, List<double>>();
            const string defaultCategory = "General"; // Use a default category for all questions

            // Track question statistics
            var questionSummaries = new Dictionary<int, QuestionSummaryDTO>();

            // Process all feedback
            foreach (var feedback in feedbacks)
            {
                foreach (var answer in feedback.Answers)
                {
                    var questionId = answer.QuestionId;
                    // Use position (order) instead of Value
                    int optionValue = GetOptionPositionAsValue(answer.SelectedOptionId);
                    var question = answer.Question ?? allQuestions.FirstOrDefault(q => q.QuestionId == questionId);

                    if (question == null)
                        continue;

                    // Add to overall rating
                    overallRatingSum += optionValue;
                    overallRatingCount++;

                    // Add to category ratings - use default category
                    if (!categoryRatings.ContainsKey(defaultCategory))
                    {
                        categoryRatings[defaultCategory] = new List<double>();
                    }
                    categoryRatings[defaultCategory].Add(optionValue);

                    // Process question statistics
                    if (!questionSummaries.ContainsKey(questionId))
                    {
                        questionSummaries[questionId] = new QuestionSummaryDTO
                        {
                            QuestionId = questionId,
                            QuestionText = question.QuestionText,
                            Category = defaultCategory,
                            AverageRating = 0,
                            OptionCounts = new List<OptionCountDTO>()
                        };
                    }

                    // Track option counts
                    var optionId = answer.SelectedOptionId;
                    var option = answer.SelectedOption;

                    var existingOptionCount = questionSummaries[questionId].OptionCounts
                        .FirstOrDefault(o => o.OptionId == optionId);

                    if (existingOptionCount == null)
                    {
                        questionSummaries[questionId].OptionCounts.Add(new OptionCountDTO
                        {
                            OptionId = optionId,
                            OptionText = option?.OptionText ?? "Unknown",
                            Count = 1,
                            Percentage = 0 // Will calculate later
                        });
                    }
                    else
                    {
                        existingOptionCount.Count++;
                    }
                }
            }

            // Calculate averages and percentages
            var overallAverage = overallRatingCount > 0 ? overallRatingSum / overallRatingCount : 0;

            var categoryAverages = new Dictionary<string, double>();
            foreach (var category in categoryRatings.Keys)
            {
                categoryAverages[category] = categoryRatings[category].Average();
            }

            foreach (var questionId in questionSummaries.Keys)
            {
                var summary = questionSummaries[questionId];

                // Calculate option percentages
                int totalResponses = summary.OptionCounts.Sum(o => o.Count);
                foreach (var option in summary.OptionCounts)
                {
                    option.Percentage = totalResponses > 0 ? (double)option.Count / totalResponses * 100 : 0;
                }

                // Calculate question average
                summary.AverageRating = totalResponses > 0
                    ? summary.OptionCounts.Sum(o => o.Count * GetOptionPositionAsValue(o.OptionId)) / totalResponses
                    : 0;
            }

            return new TeacherFeedbackSummaryDTO
            {
                TeacherId = teacherId,
                TeacherName = teacher?.Fullname ?? "Unknown",
                TotalFeedbacks = feedbacks.Count,
                OverallAverageRating = overallAverage,
                CategoryAverages = categoryAverages,
                QuestionSummaries = questionSummaries.Values.ToList()
            };
        }

        // Method to determine value based on option position
        private int GetOptionPositionAsValue(int optionId)
        {
            // Get the option and its question
            var option = _unitOfWork.LearningRegisFeedbackOptionRepository.GetByIdAsync(optionId).Result;
            if (option == null)
                return 0;

            // Get all options for the question
            var options = _unitOfWork.LearningRegisFeedbackOptionRepository.GetOptionsByQuestionIdAsync(option.QuestionId).Result;
            if (options == null || !options.Any())
                return 0;

            // Sort options by ID (natural ordering)
            var sortedOptions = options.OrderBy(o => o.OptionId).ToList();

            // Find the position (1-based index) of the current option
            int position = sortedOptions.FindIndex(o => o.OptionId == optionId) + 1;

            // Return position as the value (first option = 1, second = 2, etc.)
            return position > 0 ? position : 1;
        }

        private LearningRegisFeedbackDTO MapToFeedbackDTO(LearningRegisFeedback feedback)
        {
            if (feedback == null)
                return null;

            var feedbackDTO = _mapper.Map<LearningRegisFeedbackDTO>(feedback);

            // Calculate average rating (this is still needed since it's a calculated property)
            double totalRating = 0;
            int ratingCount = 0;

            if (feedback.Answers != null)
            {
                foreach (var answer in feedback.Answers)
                {
                    // Use the position-based value instead of the direct Value property
                    int value = GetOptionPositionAsValue(answer.SelectedOptionId);
                    totalRating += value;
                    ratingCount++;
                }
            }

            feedbackDTO.AverageRating = ratingCount > 0 ? totalRating / ratingCount : 0;

            return feedbackDTO;
        }

        private LearningRegisFeedbackQuestionDTO MapToQuestionDTO(LearningRegisFeedbackQuestion question)
        {
            return _mapper.Map<LearningRegisFeedbackQuestionDTO>(question);
        }
    }
}
