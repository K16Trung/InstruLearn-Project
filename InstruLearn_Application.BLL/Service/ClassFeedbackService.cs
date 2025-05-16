using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.ClassFeedback;
using InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ClassFeedbackService : IClassFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassFeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> CreateFeedbackAsync(CreateClassFeedbackDTO feedbackDTO)
        {
            // Verify class exists
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(feedbackDTO.ClassId);
            if (classEntity == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Class not found"
                };
            }

            // Verify learner exists
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(feedbackDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learner not found"
                };
            }

            // Check if feedback already exists
            var existingFeedback = await _unitOfWork.ClassFeedbackRepository
                .GetFeedbackByClassAndLearnerAsync(feedbackDTO.ClassId, feedbackDTO.LearnerId);

            if (existingFeedback != null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Feedback already exists for this class and learner"
                };
            }

            // Get the feedback template for this class level
            var level = await _unitOfWork.LevelAssignedRepository.GetWithIncludesAsync(
                l => l.MajorId == classEntity.MajorId);

            if (!level.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No level found for class major"
                };
            }

            var template = await _unitOfWork.LevelFeedbackTemplateRepository
                .GetTemplateForLevelAsync(level.First().LevelId);

            if (template == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No active feedback template found for this class level"
                };
            }

            // Create the feedback
            var feedback = _mapper.Map<ClassFeedback>(feedbackDTO);
            feedback.TemplateId = template.TemplateId;
            feedback.CreatedAt = DateTime.Now;
            feedback.CompletedAt = DateTime.Now;

            await _unitOfWork.ClassFeedbackRepository.AddAsync(feedback);
            await _unitOfWork.SaveChangeAsync();

            // Get the newly created feedback ID
            var createdFeedback = await _unitOfWork.ClassFeedbackRepository.GetFeedbackByClassAndLearnerAsync(
                feedbackDTO.ClassId, feedbackDTO.LearnerId);

            // Add evaluations
            if (feedbackDTO.Evaluations != null && feedbackDTO.Evaluations.Any())
            {
                foreach (var evaluationDTO in feedbackDTO.Evaluations)
                {
                    // Verify criterion exists and belongs to the template
                    var criterion = await _unitOfWork.LevelFeedbackCriterionRepository.GetByIdAsync(evaluationDTO.CriterionId);
                    if (criterion == null || criterion.TemplateId != template.TemplateId)
                    {
                        continue; // Skip invalid criteria
                    }

                    var evaluation = _mapper.Map<ClassFeedbackEvaluation>(evaluationDTO);
                    evaluation.FeedbackId = createdFeedback.FeedbackId;

                    await _unitOfWork.ClassFeedbackEvaluationRepository.AddAsync(evaluation);
                }

                await _unitOfWork.SaveChangeAsync();
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback created successfully",
                Data = createdFeedback.FeedbackId
            };
        }

        public async Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateClassFeedbackDTO feedbackDTO)
        {
            var feedback = await _unitOfWork.ClassFeedbackRepository.GetFeedbackWithEvaluationsAsync(feedbackId);
            if (feedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Feedback not found"
                };
            }

            // Update basic properties
            feedback.AdditionalComments = feedbackDTO.AdditionalComments;
            feedback.CompletedAt = DateTime.Now;

            await _unitOfWork.ClassFeedbackRepository.UpdateAsync(feedback);

            // Handle evaluations updates
            if (feedbackDTO.Evaluations != null)
            {
                var existingEvaluations = await _unitOfWork.ClassFeedbackEvaluationRepository
                    .GetEvaluationsByFeedbackIdAsync(feedbackId);
                var existingIds = existingEvaluations.Select(e => e.EvaluationId).ToList();
                var updatedIds = feedbackDTO.Evaluations
                    .Where(e => e.EvaluationId > 0)
                    .Select(e => e.EvaluationId)
                    .ToList();

                // Delete evaluations not in the updated list
                foreach (var evaluationId in existingIds)
                {
                    if (!updatedIds.Contains(evaluationId))
                    {
                        await _unitOfWork.ClassFeedbackEvaluationRepository.DeleteAsync(evaluationId);
                    }
                }

                // Update or add evaluations
                foreach (var evaluationDTO in feedbackDTO.Evaluations)
                {
                    if (evaluationDTO.EvaluationId > 0)
                    {
                        // Update existing evaluation
                        var evaluation = await _unitOfWork.ClassFeedbackEvaluationRepository
                            .GetByIdAsync(evaluationDTO.EvaluationId);

                        if (evaluation != null)
                        {
                            evaluation.AchievedPercentage = evaluationDTO.AchievedPercentage;
                            evaluation.Comment = evaluationDTO.Comment;

                            await _unitOfWork.ClassFeedbackEvaluationRepository.UpdateAsync(evaluation);
                        }
                    }
                    else
                    {
                        // Verify criterion exists and belongs to the feedback's template
                        var criterion = await _unitOfWork.LevelFeedbackCriterionRepository
                            .GetByIdAsync(evaluationDTO.CriterionId);

                        if (criterion != null && criterion.TemplateId == feedback.TemplateId)
                        {
                            // Add new evaluation
                            var newEvaluation = new ClassFeedbackEvaluation
                            {
                                FeedbackId = feedbackId,
                                CriterionId = evaluationDTO.CriterionId,
                                AchievedPercentage = evaluationDTO.AchievedPercentage,
                                Comment = evaluationDTO.Comment
                            };

                            await _unitOfWork.ClassFeedbackEvaluationRepository.AddAsync(newEvaluation);
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback updated successfully"
            };
        }

        public async Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _unitOfWork.ClassFeedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Feedback not found"
                };
            }

            // Delete evaluations first
            var evaluations = await _unitOfWork.ClassFeedbackEvaluationRepository
                .GetEvaluationsByFeedbackIdAsync(feedbackId);

            foreach (var evaluation in evaluations)
            {
                await _unitOfWork.ClassFeedbackEvaluationRepository.DeleteAsync(evaluation.EvaluationId);
            }

            // Delete feedback
            await _unitOfWork.ClassFeedbackRepository.DeleteAsync(feedbackId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback deleted successfully"
            };
        }

        public async Task<ClassFeedbackDTO> GetFeedbackAsync(int feedbackId)
        {
            var feedback = await _unitOfWork.ClassFeedbackRepository.GetFeedbackWithEvaluationsAsync(feedbackId);
            if (feedback == null)
                return null;

            var feedbackDto = _mapper.Map<ClassFeedbackDTO>(feedback);

            // Calculate total percentage based on achievements
            if (feedback.Evaluations != null && feedback.Evaluations.Any())
            {
                decimal totalPercentage = 0;

                foreach (var evaluation in feedback.Evaluations)
                {
                    totalPercentage += evaluation.AchievedPercentage;
                }

                feedbackDto.AverageScore = totalPercentage;
            }
            else
            {
                feedbackDto.AverageScore = 0;
            }

            return feedbackDto;
        }

        public async Task<List<ClassFeedbackDTO>> GetFeedbacksByClassIdAsync(int classId)
        {
            var feedbacks = await _unitOfWork.ClassFeedbackRepository.GetFeedbacksByClassIdAsync(classId);
            return _mapper.Map<List<ClassFeedbackDTO>>(feedbacks);
        }

        public async Task<List<ClassFeedbackDTO>> GetFeedbacksByLearnerIdAsync(int learnerId)
        {
            var feedbacks = await _unitOfWork.ClassFeedbackRepository.GetFeedbacksByLearnerIdAsync(learnerId);
            var feedbackDTOs = _mapper.Map<List<ClassFeedbackDTO>>(feedbacks);

            foreach (var feedbackDTO in feedbackDTOs)
            {
                var feedbackWithEval = await _unitOfWork.ClassFeedbackRepository.GetFeedbackWithEvaluationsAsync(feedbackDTO.FeedbackId);
                if (feedbackWithEval?.Evaluations == null || !feedbackWithEval.Evaluations.Any())
                {
                    feedbackDTO.AverageScore = 0;
                    continue;
                }

                decimal totalPercentage = 0;

                foreach (var evaluation in feedbackWithEval.Evaluations)
                {
                    totalPercentage += evaluation.AchievedPercentage;
                }

                feedbackDTO.AverageScore = totalPercentage;
            }

            return feedbackDTOs;
        }

        public async Task<ClassFeedbackDTO> GetFeedbackByClassAndLearnerAsync(int classId, int learnerId)
        {
            var feedback = await _unitOfWork.ClassFeedbackRepository
        .GetFeedbackByClassAndLearnerAsync(classId, learnerId);

            if (feedback != null)
            {
                var feedbackDto = _mapper.Map<ClassFeedbackDTO>(feedback);

                var feedbackWithEval = await _unitOfWork.ClassFeedbackRepository
                    .GetFeedbackWithEvaluationsAsync(feedback.FeedbackId);

                if (feedbackWithEval?.Evaluations != null && feedbackWithEval.Evaluations.Any())
                {
                    decimal totalPercentage = 0;

                    foreach (var evaluation in feedbackWithEval.Evaluations)
                    {
                        // Just add the achieved percentage directly - no null check needed for decimal (non-nullable)
                        totalPercentage += evaluation.AchievedPercentage;
                    }

                    feedbackDto.AverageScore = totalPercentage;
                }
                else
                {
                    feedbackDto.AverageScore = 0;
                }

                // Check if feedback is not completed or has incomplete evaluations
                bool isIncomplete = feedback.CompletedAt == null ||
                    (feedbackWithEval?.Evaluations == null ||
                     !feedbackWithEval.Evaluations.Any());

                if (isIncomplete)
                {
                    // Get class to access level information (avoid name conflict)
                    var existingClass = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
                    if (existingClass != null)
                    {
                        // Get template directly using the TemplateId from feedback (avoid name conflict)
                        var existingTemplate = await _unitOfWork.LevelFeedbackTemplateRepository
                            .GetTemplateWithCriteriaAsync(feedback.TemplateId);

                        if (existingTemplate != null)
                        {

                            // Ensure template name is set
                            if (string.IsNullOrEmpty(feedbackDto.TemplateName))
                            {
                                feedbackDto.TemplateName = existingTemplate.TemplateName;
                            }

                            // Process criteria/evaluations
                            if (existingTemplate.Criteria != null)
                            {
                                // Ensure evaluations list exists
                                if (feedbackDto.Evaluations == null)
                                {
                                    feedbackDto.Evaluations = new List<ClassFeedbackEvaluationDTO>();
                                }

                                // Add missing criteria from template
                                foreach (var criterion in existingTemplate.Criteria.OrderBy(c => c.DisplayOrder))
                                {
                                    // Check if this criterion already exists in the feedback
                                    var existingEvaluation = feedbackDto.Evaluations
                                        .FirstOrDefault(e => e.CriterionId == criterion.CriterionId);

                                    if (existingEvaluation == null)
                                    {
                                        // Add the missing criterion with null achievement
                                        feedbackDto.Evaluations.Add(new ClassFeedbackEvaluationDTO
                                        {
                                            EvaluationId = 0,
                                            CriterionId = criterion.CriterionId,
                                            GradeCategory = criterion.GradeCategory,
                                            Description = criterion.Description,
                                            Weight = criterion.Weight,
                                            AchievedPercentage = null,
                                            Comment = null
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                return feedbackDto;
            }

            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classEntity == null)
                return null;

            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            if (learner == null)
                return null;

            var learnerSchedules = await _unitOfWork.ScheduleRepository
                .GetClassSchedulesByLearnerIdAsync(learnerId);

            bool isEnrolled = learnerSchedules != null &&
                              learnerSchedules.Any(s => s.ClassId == classId);

            if (!isEnrolled)
                return null;

            var template = await _unitOfWork.LevelFeedbackTemplateRepository
                .GetTemplateForLevelAsync(classEntity.LevelId.Value);

            if (template == null)
                return null;

            // Create a template-based DTO with complete information
            var templateBasedDto = new ClassFeedbackDTO
            {
                FeedbackId = 0,
                ClassId = classId,
                ClassName = classEntity.ClassName,
                LearnerId = learnerId,
                LearnerName = learner.FullName,
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                CreatedAt = DateTime.MinValue,
                CompletedAt = null,
                AdditionalComments = null,
                AverageScore = 0,
                Evaluations = new List<ClassFeedbackEvaluationDTO>()
            };

            // Add criteria from template with null achievements
            if (template.Criteria != null)
            {
                foreach (var criterion in template.Criteria.OrderBy(c => c.DisplayOrder))
                {
                    templateBasedDto.Evaluations.Add(new ClassFeedbackEvaluationDTO
                    {
                        EvaluationId = 0,
                        CriterionId = criterion.CriterionId,
                        GradeCategory = criterion.GradeCategory,
                        Description = criterion.Description,
                        Weight = criterion.Weight,
                        AchievedPercentage = null,
                        Comment = null
                    });
                }
            }

            return templateBasedDto;
        }

        public async Task<ClassFeedbackSummaryDTO> GetFeedbackSummaryForClassAsync(int classId)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classEntity == null)
                return null;

            var feedbacks = await _unitOfWork.ClassFeedbackRepository.GetFeedbacksByClassIdAsync(classId);
            if (feedbacks == null || !feedbacks.Any())
            {
                return new ClassFeedbackSummaryDTO
                {
                    ClassId = classId,
                    ClassName = classEntity.ClassName,
                    MajorName = classEntity.Major?.MajorName ?? "Unknown",
                    LevelName = "Unknown",
                    TotalFeedbacks = 0,
                    OverallAverageScore = 0,
                    CriterionSummaries = new List<CriterionSummaryDTO>()
                };
            }

            var criterionTotals = new Dictionary<int, decimal>();
            var criterionCounts = new Dictionary<int, int>();
            var criterionDetails = new Dictionary<int, (string Name, decimal Weight)>();

            foreach (var feedback in feedbacks)
            {
                foreach (var evaluation in feedback.Evaluations)
                {
                    int criterionId = evaluation.CriterionId;
                    
                    if (!criterionTotals.ContainsKey(criterionId))
                    {
                        criterionTotals[criterionId] = 0;
                        criterionCounts[criterionId] = 0;
                        criterionDetails[criterionId] = (evaluation.Criterion.GradeCategory, evaluation.Criterion.Weight);
                    }

                    criterionTotals[criterionId] += evaluation.AchievedPercentage;
                    criterionCounts[criterionId]++;
                }
            }

            decimal overallTotal = criterionTotals.Values.Sum();

            var criterionSummaries = new List<CriterionSummaryDTO>();
            foreach (var criterionId in criterionTotals.Keys)
            {
                criterionSummaries.Add(new CriterionSummaryDTO
                {
                    CriterionId = criterionId,
                    GradeCategory = criterionDetails[criterionId].Name,
                    Weight = criterionDetails[criterionId].Weight,
                    AverageScore = criterionTotals[criterionId]
                });
            }

            string levelName = "Unknown";
            if (classEntity.Major != null)
            {
                var levels = await _unitOfWork.LevelAssignedRepository.GetWithIncludesAsync(l => l.MajorId == classEntity.MajorId);
                if (levels.Any())
                {
                    levelName = levels.First().LevelName;
                }
            }

            return new ClassFeedbackSummaryDTO
            {
                ClassId = classId,
                ClassName = classEntity.ClassName,
                MajorName = classEntity.Major?.MajorName ?? "Unknown",
                LevelName = levelName,
                TotalFeedbacks = feedbacks.Count(),
                OverallAverageScore = overallTotal,
                CriterionSummaries = criterionSummaries
            };
        }
    }
}
