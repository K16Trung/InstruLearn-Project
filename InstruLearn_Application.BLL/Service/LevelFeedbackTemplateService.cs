using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.LevelFeedbackTemplate;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LevelFeedbackTemplateService : ILevelFeedbackTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LevelFeedbackTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> CreateTemplateAsync(CreateLevelFeedbackTemplateDTO templateDTO)
        {
            var level = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(templateDTO.LevelId);
            if (level == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy cấp độ"
                };
            }

            if (templateDTO.Criteria != null && templateDTO.Criteria.Any())
            {
                var uniqueCriteria = templateDTO.Criteria
                    .GroupBy(c => c.GradeCategory)
                    .Select(g => g.First())
                    .ToList();

                if (uniqueCriteria.Count < templateDTO.Criteria.Count)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Đã phát hiện tiêu chí đánh giá trùng lặp. Vui lòng đảm bảo tất cả các tiêu chí là duy nhất."
                    };
                }
            }

            var existingTemplate = await _unitOfWork.LevelFeedbackTemplateRepository.GetTemplateForLevelAsync(templateDTO.LevelId);
            if (existingTemplate != null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Đã tồn tại một mẫu phản hồi đang hoạt động cho cấp độ này"
                };
            }

            using (var transaction = await _unitOfWork.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var template = new LevelFeedbackTemplate
                    {
                        LevelId = templateDTO.LevelId,
                        TemplateName = templateDTO.TemplateName,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.LevelFeedbackTemplateRepository.AddAsync(template);
                    await _unitOfWork.SaveChangeAsync();

                    var createdTemplate = await _unitOfWork.LevelFeedbackTemplateRepository.GetAsync(
                        t => t.LevelId == templateDTO.LevelId && t.TemplateName == templateDTO.TemplateName);

                    if (templateDTO.Criteria != null && templateDTO.Criteria.Any())
                    {
                        var criteriaToAdd = new List<LevelFeedbackCriterion>();

                        foreach (var criterionDTO in templateDTO.Criteria)
                        {
                            var criterion = _mapper.Map<LevelFeedbackCriterion>(criterionDTO);
                            criterion.TemplateId = createdTemplate.TemplateId;
                            criteriaToAdd.Add(criterion);
                        }

                        foreach (var criterion in criteriaToAdd)
                        {
                            await _unitOfWork.LevelFeedbackCriterionRepository.AddAsync(criterion);
                        }

                        await _unitOfWork.SaveChangeAsync();
                    }

                    await transaction.CommitAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã tạo mẫu phản hồi thành công",
                        Data = createdTemplate.TemplateId
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Lỗi khi tạo mẫu: {ex.Message}"
                    };
                }
            }
        }

        public async Task<ResponseDTO> UpdateTemplateAsync(int templateId, UpdateLevelFeedbackTemplateDTO templateDTO)
        {
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetTemplateWithCriteriaAsync(templateId);
            if (template == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy mẫu"
                };
            }

            template.TemplateName = templateDTO.TemplateName;
            template.IsActive = templateDTO.IsActive;

            await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(template);

            if (templateDTO.Criteria != null)
            {
                var existingCriteria = await _unitOfWork.LevelFeedbackCriterionRepository
                    .GetCriteriaByTemplateIdAsync(templateId);
                var existingIds = existingCriteria.Select(c => c.CriterionId).ToList();
                var updatedIds = templateDTO.Criteria
                    .Where(c => c.CriterionId > 0)
                    .Select(c => c.CriterionId)
                    .ToList();

                foreach (var criterionId in existingIds)
                {
                    if (!updatedIds.Contains(criterionId))
                    {
                        await _unitOfWork.LevelFeedbackCriterionRepository.DeleteAsync(criterionId);
                    }
                }

                int order = 1;
                foreach (var criterionDTO in templateDTO.Criteria)
                {
                    if (criterionDTO.CriterionId > 0)
                    {
                        var criterion = existingCriteria.FirstOrDefault(c => c.CriterionId == criterionDTO.CriterionId);
                        if (criterion != null)
                        {
                            criterion.GradeCategory = criterionDTO.GradeCategory;
                            criterion.Weight = criterionDTO.Weight;
                            criterion.Description = criterionDTO.Description;
                            criterion.DisplayOrder = order++;

                            await _unitOfWork.LevelFeedbackCriterionRepository.UpdateAsync(criterion);
                        }
                    }
                    else
                    {
                        var newCriterion = new LevelFeedbackCriterion
                        {
                            TemplateId = templateId,
                            GradeCategory = criterionDTO.GradeCategory,
                            Weight = criterionDTO.Weight,
                            Description = criterionDTO.Description,
                            DisplayOrder = order++
                        };

                        await _unitOfWork.LevelFeedbackCriterionRepository.AddAsync(newCriterion);
                    }
                }
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã cập nhật mẫu phản hồi thành công"
            };
        }

        public async Task<ResponseDTO> DeleteTemplateAsync(int templateId)
        {
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy mẫu"
                };
            }

            var feedbacks = await _unitOfWork.ClassFeedbackRepository.GetAllAsync(f => f.TemplateId == templateId);
            if (feedbacks != null && feedbacks.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không thể xóa mẫu vì nó đang được sử dụng trong các mục phản hồi hiện có"
                };
            }

            var criteria = await _unitOfWork.LevelFeedbackCriterionRepository.GetCriteriaByTemplateIdAsync(templateId);
            foreach (var criterion in criteria)
            {
                await _unitOfWork.LevelFeedbackCriterionRepository.DeleteAsync(criterion.CriterionId);
            }

            await _unitOfWork.LevelFeedbackTemplateRepository.DeleteAsync(templateId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã xóa mẫu phản hồi thành công"
            };
        }

        public async Task<ResponseDTO> ActivateTemplateAsync(int templateId)
        {
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy mẫu"
                };
            }

            var activeTemplates = await _unitOfWork.LevelFeedbackTemplateRepository.GetAllAsync(
                t => t.LevelId == template.LevelId && t.IsActive && t.TemplateId != templateId);

            foreach (var activeTemplate in activeTemplates)
            {
                activeTemplate.IsActive = false;
                await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(activeTemplate);
            }

            template.IsActive = true;
            await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(template);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã kích hoạt mẫu phản hồi thành công"
            };
        }

        public async Task<ResponseDTO> DeactivateTemplateAsync(int templateId)
        {
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy mẫu"
                };
            }

            template.IsActive = false;
            await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(template);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã vô hiệu hóa mẫu phản hồi thành công"
            };
        }

        public async Task<LevelFeedbackTemplateDTO> GetTemplateAsync(int templateId)
        {
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetTemplateWithCriteriaAsync(templateId);
            if (template == null)
                return null;

            return _mapper.Map<LevelFeedbackTemplateDTO>(template);
        }

        public async Task<List<LevelFeedbackTemplateDTO>> GetAllTemplatesAsync()
        {
            var templates = await _unitOfWork.LevelFeedbackTemplateRepository.GetAllTemplatesWithCriteriaAsync();
            return _mapper.Map<List<LevelFeedbackTemplateDTO>>(templates);
        }

        public async Task<LevelFeedbackTemplateDTO> GetTemplateForLevelAsync(int levelId)
        {
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetTemplateForLevelAsync(levelId);
            if (template == null)
                return null;

            return _mapper.Map<LevelFeedbackTemplateDTO>(template);
        }
    }

}
