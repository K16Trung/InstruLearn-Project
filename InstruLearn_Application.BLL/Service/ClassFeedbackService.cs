using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Certification;
using InstruLearn_Application.Model.Models.DTO.ClassFeedback;
using InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation;
using InstruLearn_Application.Model.Models.DTO.Feedback;
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
        private readonly IGoogleSheetsService _googleSheetsService;

        public ClassFeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IGoogleSheetsService googleSheetsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _googleSheetsService = googleSheetsService;
        }
        public async Task<ResponseDTO> CreateFeedbackAsync(CreateClassFeedbackDTO feedbackDTO)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(feedbackDTO.ClassId);
            if (classEntity == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Class not found"
                };
            }

            var classDayValues = classEntity.ClassDays.Select(cd => cd.Day).ToList();
            if (!classDayValues.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Class schedule information not found"
                };
            }

            var endDate = DateTimeHelper.CalculateEndDate(classEntity.StartDate, classEntity.totalDays, classDayValues);
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (today != endDate)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Phản hồi chỉ có thể được tạo vào ngày cuối cùng của lớp học ({endDate:yyyy-MM-dd})"
                };
            }

            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(feedbackDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learner not found"
                };
            }

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

            int templateId = 0;

            if (feedbackDTO.Evaluations != null && feedbackDTO.Evaluations.Any())
            {
                var firstCriterionId = feedbackDTO.Evaluations.First().CriterionId;
                var criterion = await _unitOfWork.LevelFeedbackCriterionRepository.GetByIdAsync(firstCriterionId);

                if (criterion != null)
                {
                    templateId = criterion.TemplateId;
                }
            }

            if (templateId == 0)
            {
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

                templateId = template.TemplateId;
            }

            var feedback = _mapper.Map<ClassFeedback>(feedbackDTO);
            feedback.TemplateId = templateId;
            feedback.CreatedAt = DateTime.Now;
            feedback.CompletedAt = DateTime.Now;

            await _unitOfWork.ClassFeedbackRepository.AddAsync(feedback);
            await _unitOfWork.SaveChangeAsync();

            var createdFeedback = await _unitOfWork.ClassFeedbackRepository.GetFeedbackByClassAndLearnerAsync(
                feedbackDTO.ClassId, feedbackDTO.LearnerId);

            if (createdFeedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to retrieve created feedback"
                };
            }

            decimal totalPercentage = 0;
            decimal totalWeight = 0;

            var criteria = await _unitOfWork.LevelFeedbackCriterionRepository.GetCriteriaByTemplateIdAsync(templateId);
            if (criteria != null)
            {
                totalWeight = criteria.Sum(c => c.Weight);
            }

            if (feedbackDTO.Evaluations != null && feedbackDTO.Evaluations.Any())
            {
                foreach (var evaluationDTO in feedbackDTO.Evaluations)
                {
                    var criterion = await _unitOfWork.LevelFeedbackCriterionRepository.GetByIdAsync(evaluationDTO.CriterionId);
                    if (criterion == null)
                        continue;

                    var evaluation = new ClassFeedbackEvaluation
                    {
                        FeedbackId = createdFeedback.FeedbackId,
                        CriterionId = evaluationDTO.CriterionId,
                        AchievedPercentage = evaluationDTO.AchievedPercentage ?? 0,
                        Comment = evaluationDTO.Comment
                    };

                    if (evaluationDTO.AchievedPercentage.HasValue)
                    {
                        totalPercentage += evaluationDTO.AchievedPercentage.Value;
                    }

                    await _unitOfWork.ClassFeedbackEvaluationRepository.AddAsync(evaluation);
                }

                await _unitOfWork.SaveChangeAsync();
            }

            if (totalPercentage > 50)
            {
                var existingCertificates = await _unitOfWork.CertificationRepository.GetByLearnerIdAsync(feedbackDTO.LearnerId);
                var existingCertForClass = existingCertificates.FirstOrDefault(c =>
                    c.CertificationType == CertificationType.CenterLearning &&
                    c.CertificationName != null &&
                    c.CertificationName.Contains(classEntity.ClassId.ToString()));

                bool isTemporaryCert = existingCertForClass != null &&
                    existingCertForClass.CertificationName.Contains("[TEMPORARY]");

                string teacherName = "Unknown Teacher";
                if (classEntity.Teacher != null)
                {
                    teacherName = classEntity.Teacher.Fullname;
                }
                else
                {
                    var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(classEntity.TeacherId);
                    if (teacher != null)
                    {
                        teacherName = teacher.Fullname;
                    }
                }

                string majorName = "Unknown Subject";
                if (classEntity.Major != null)
                {
                    majorName = classEntity.Major.MajorName;
                }
                else
                {
                    var major = await _unitOfWork.MajorRepository.GetByIdAsync(classEntity.MajorId);
                    if (major != null)
                    {
                        majorName = major.MajorName;
                    }
                }

                if (existingCertForClass == null)
                {
                    var certification = new Certification
                    {
                        LearnerId = feedbackDTO.LearnerId,
                        CertificationType = CertificationType.CenterLearning,
                        CertificationName = $"Center Learning Certificate - {classEntity.ClassName} (Class ID: {classEntity.ClassId})",
                        TeacherName = teacherName,
                        Subject = majorName,
                        IssueDate = DateTime.Now
                    };

                    await _unitOfWork.CertificationRepository.AddAsync(certification);
                    await _unitOfWork.SaveChangeAsync();

                    try
                    {
                        var certificationData = new CertificationDataDTO
                        {
                            CertificationId = certification.CertificationId,
                            LearnerName = learner.FullName,
                            LearnerEmail = learner.FullName,
                            CertificationType = certification.CertificationType.ToString(),
                            CertificationName = certification.CertificationName,
                            IssueDate = certification.IssueDate,
                            TeacherName = certification.TeacherName,
                            Subject = certification.Subject,
                            FileStatus = String.Empty,
                            FileLink = String.Empty
                        };

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _googleSheetsService.SaveCertificationDataAsync(certificationData);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error saving certificate to Google Sheets: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error preparing certificate for Google Sheets: {ex.Message}");
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Feedback created successfully and certification issued due to high score",
                        Data = new
                        {
                            FeedbackId = createdFeedback.FeedbackId,
                            CertificationIssued = true,
                            Score = totalPercentage
                        }
                    };
                }
                else if (isTemporaryCert)
                {
                    existingCertForClass.CertificationName = existingCertForClass.CertificationName.Replace("[TEMPORARY] ", "");
                    existingCertForClass.IssueDate = DateTime.Now;

                    await _unitOfWork.CertificationRepository.UpdateAsync(existingCertForClass);
                    await _unitOfWork.SaveChangeAsync();

                    try
                    {
                        var certificationData = new CertificationDataDTO
                        {
                            CertificationId = existingCertForClass.CertificationId,
                            LearnerName = learner.FullName,
                            LearnerEmail = learner.FullName,
                            CertificationType = existingCertForClass.CertificationType.ToString(),
                            CertificationName = existingCertForClass.CertificationName,
                            IssueDate = existingCertForClass.IssueDate,
                            TeacherName = existingCertForClass.TeacherName,
                            Subject = existingCertForClass.Subject,
                            FileStatus = String.Empty,
                            FileLink = String.Empty
                        };

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _googleSheetsService.SaveCertificationDataAsync(certificationData);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error saving certificate to Google Sheets: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error preparing certificate for Google Sheets: {ex.Message}");
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Feedback created successfully and temporary certification upgraded to permanent due to high score",
                        Data = new
                        {
                            FeedbackId = createdFeedback.FeedbackId,
                            CertificationUpgraded = true,
                            Score = totalPercentage
                        }
                    };
                }
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback thành công",
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

            feedback.AdditionalComments = feedbackDTO.AdditionalComments;
            feedback.CompletedAt = DateTime.Now;

            await _unitOfWork.ClassFeedbackRepository.UpdateAsync(feedback);

            if (feedbackDTO.Evaluations != null)
            {
                var existingEvaluations = await _unitOfWork.ClassFeedbackEvaluationRepository
                    .GetEvaluationsByFeedbackIdAsync(feedbackId);
                var existingIds = existingEvaluations.Select(e => e.EvaluationId).ToList();
                var updatedIds = feedbackDTO.Evaluations
                    .Where(e => e.EvaluationId > 0)
                    .Select(e => e.EvaluationId)
                    .ToList();

                foreach (var evaluationId in existingIds)
                {
                    if (!updatedIds.Contains(evaluationId))
                    {
                        await _unitOfWork.ClassFeedbackEvaluationRepository.DeleteAsync(evaluationId);
                    }
                }

                foreach (var evaluationDTO in feedbackDTO.Evaluations)
                {
                    if (evaluationDTO.EvaluationId > 0)
                    {
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
                        var criterion = await _unitOfWork.LevelFeedbackCriterionRepository
                            .GetByIdAsync(evaluationDTO.CriterionId);

                        if (criterion != null && criterion.TemplateId == feedback.TemplateId)
                        {
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

            var evaluations = await _unitOfWork.ClassFeedbackEvaluationRepository
                .GetEvaluationsByFeedbackIdAsync(feedbackId);

            foreach (var evaluation in evaluations)
            {
                await _unitOfWork.ClassFeedbackEvaluationRepository.DeleteAsync(evaluation.EvaluationId);
            }

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
            var feedbackDTOs = _mapper.Map<List<ClassFeedbackDTO>>(feedbacks);

            foreach (var feedbackDTO in feedbackDTOs)
            {
                var feedbackWithEval = await _unitOfWork.ClassFeedbackRepository.GetFeedbackWithEvaluationsAsync(feedbackDTO.FeedbackId);
                if (feedbackWithEval?.Evaluations == null || !feedbackWithEval.Evaluations.Any())
                {
                    feedbackDTO.AverageScore = 0;
                    feedbackDTO.TotalWeight = 0;
                    continue;
                }

                decimal totalPercentage = 0;
                decimal totalWeight = 0;

                foreach (var evaluation in feedbackWithEval.Evaluations)
                {
                    totalPercentage += evaluation.AchievedPercentage;
                    totalWeight += evaluation.Criterion.Weight;
                }

                feedbackDTO.AverageScore = totalPercentage;
                feedbackDTO.TotalWeight = totalWeight;
            }

            return feedbackDTOs;
        }

        public async Task<List<ClassFeedbackDTO>> GetFeedbacksByLearnerIdAsync(int learnerId)
        {
            var feedbacks = await _unitOfWork.ClassFeedbackRepository.GetFeedbacksByLearnerIdAsync(learnerId);
            var feedbackDTOs = _mapper.Map<List<ClassFeedbackDTO>>(feedbacks);

            foreach (var feedbackDTO in feedbackDTOs)
            {
                var feedbackWithEval = await _unitOfWork.ClassFeedbackRepository.GetFeedbackWithEvaluationsAsync(feedbackDTO.FeedbackId);
                if (feedbackWithEval == null)
                    continue;

                var template = await _unitOfWork.LevelFeedbackTemplateRepository
                    .GetTemplateWithCriteriaAsync(feedbackWithEval.TemplateId);

                if (template?.Criteria != null)
                {
                    feedbackDTO.TotalWeight = template.Criteria.Sum(c => c.Weight);
                }
                else
                {
                    feedbackDTO.TotalWeight = 0;
                }

                decimal totalPercentage = 0;

                feedbackDTO.Evaluations = new List<ClassFeedbackEvaluationDTO>();

                bool hasEvaluations = false;

                var evaluations = await _unitOfWork.ClassFeedbackEvaluationRepository
                    .GetEvaluationsByFeedbackIdAsync(feedbackDTO.FeedbackId);

                if (evaluations != null && evaluations.Any())
                {
                    hasEvaluations = true;
                    foreach (var evaluation in evaluations)
                    {
                        totalPercentage += evaluation.AchievedPercentage;

                        var criterion = await _unitOfWork.LevelFeedbackCriterionRepository.GetByIdAsync(evaluation.CriterionId);

                        feedbackDTO.Evaluations.Add(new ClassFeedbackEvaluationDTO
                        {
                            EvaluationId = evaluation.EvaluationId,
                            CriterionId = evaluation.CriterionId,
                            Description = criterion?.Description,
                            GradeCategory = criterion?.GradeCategory,
                            Weight = criterion?.Weight ?? 0,
                            AchievedPercentage = evaluation.AchievedPercentage,
                            Comment = evaluation.Comment
                        });
                    }
                }
                else if (!hasEvaluations && template?.Criteria != null)
                {
                    foreach (var criterion in template.Criteria.OrderBy(c => c.DisplayOrder))
                    {
                        feedbackDTO.Evaluations.Add(new ClassFeedbackEvaluationDTO
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

                feedbackDTO.TeacherId = feedbackWithEval.Class?.TeacherId ?? 0;
                feedbackDTO.TeacherName = feedbackWithEval.Class?.Teacher?.Fullname ?? "Unknown";
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

                var existingTemplate = await _unitOfWork.LevelFeedbackTemplateRepository
                    .GetTemplateWithCriteriaAsync(feedback.TemplateId);

                if (existingTemplate?.Criteria != null)
                {
                    feedbackDto.TotalWeight = existingTemplate.Criteria.Sum(c => c.Weight);
                }
                else
                {
                    feedbackDto.TotalWeight = 0;
                }

                if (feedbackDto.Evaluations == null)
                {
                    feedbackDto.Evaluations = new List<ClassFeedbackEvaluationDTO>();
                }
                else
                {
                    feedbackDto.Evaluations.Clear();
                }

                decimal totalPercentage = 0;

                if (feedbackWithEval?.Evaluations != null && feedbackWithEval.Evaluations.Any())
                {
                    foreach (var evaluation in feedbackWithEval.Evaluations)
                    {
                        totalPercentage += evaluation.AchievedPercentage;

                        feedbackDto.Evaluations.Add(new ClassFeedbackEvaluationDTO
                        {
                            EvaluationId = evaluation.EvaluationId,
                            CriterionId = evaluation.CriterionId,
                            Description = evaluation.Criterion?.Description,
                            GradeCategory = evaluation.Criterion?.GradeCategory,
                            Weight = evaluation.Criterion?.Weight ?? 0,
                            AchievedPercentage = evaluation.AchievedPercentage,
                            Comment = evaluation.Comment
                        });
                    }
                }

                feedbackDto.AverageScore = totalPercentage;

                if (feedbackWithEval?.Class != null)
                {
                    feedbackDto.TeacherId = feedbackWithEval.Class.TeacherId;
                    feedbackDto.TeacherName = feedbackWithEval.Class.Teacher?.Fullname ?? "Unknown";
                }

                bool isIncomplete = feedback.CompletedAt == null ||
                    (feedbackWithEval?.Evaluations == null ||
                     !feedbackWithEval.Evaluations.Any());

                if (isIncomplete)
                {
                    var existingClass = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
                    if (existingClass != null)
                    {
                        if (existingTemplate?.Criteria != null)
                        {
                            if (string.IsNullOrEmpty(feedbackDto.TemplateName))
                            {
                                feedbackDto.TemplateName = existingTemplate.TemplateName;
                            }

                            if (feedbackDto.Evaluations == null)
                            {
                                feedbackDto.Evaluations = new List<ClassFeedbackEvaluationDTO>();
                            }

                            foreach (var criterion in existingTemplate.Criteria.OrderBy(c => c.DisplayOrder))
                            {
                                var existingEvaluation = feedbackDto.Evaluations
                                    .FirstOrDefault(e => e.CriterionId == criterion.CriterionId);

                                if (existingEvaluation == null)
                                {
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
                Evaluations = new List<ClassFeedbackEvaluationDTO>(),
                TotalWeight = 0
            };

            if (template.Criteria != null)
            {
                templateBasedDto.TotalWeight = template.Criteria.Sum(c => c.Weight);
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
