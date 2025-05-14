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
            // Check if level exists
            var level = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(templateDTO.LevelId);
            if (level == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Level not found"
                };
            }

            // Check for duplicate criteria
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
                        Message = "Duplicate criteria grade categories detected. Please ensure all criteria are unique."
                    };
                }
            }

            // Check if a template already exists for this level and is active
            var existingTemplate = await _unitOfWork.LevelFeedbackTemplateRepository.GetTemplateForLevelAsync(templateDTO.LevelId);
            if (existingTemplate != null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "An active feedback template already exists for this level"
                };
            }

            // Create a complete template with criteria in a single transaction
            using (var transaction = await _unitOfWork.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Create the template
                    var template = new LevelFeedbackTemplate
                    {
                        LevelId = templateDTO.LevelId,
                        TemplateName = templateDTO.TemplateName,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    // Add template to context but don't save yet
                    await _unitOfWork.LevelFeedbackTemplateRepository.AddAsync(template);
                    await _unitOfWork.SaveChangeAsync();

                    // Get the ID of the newly created template
                    var createdTemplate = await _unitOfWork.LevelFeedbackTemplateRepository.GetAsync(
                        t => t.LevelId == templateDTO.LevelId && t.TemplateName == templateDTO.TemplateName);

                    // Add criteria to the template if any
                    if (templateDTO.Criteria != null && templateDTO.Criteria.Any())
                    {
                        // Create a list of criteria to add in a single operation
                        var criteriaToAdd = new List<LevelFeedbackCriterion>();

                        foreach (var criterionDTO in templateDTO.Criteria)
                        {
                            var criterion = _mapper.Map<LevelFeedbackCriterion>(criterionDTO);
                            criterion.TemplateId = createdTemplate.TemplateId;
                            // Use the display order from the DTO
                            criteriaToAdd.Add(criterion);
                        }

                        // Add all criteria to the repository
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
                        Message = "Feedback template created successfully",
                        Data = createdTemplate.TemplateId
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Error creating template: {ex.Message}"
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
                    Message = "Template not found"
                };
            }

            // Update template properties
            template.TemplateName = templateDTO.TemplateName;
            template.IsActive = templateDTO.IsActive;

            await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(template);

            // Handle criteria updates
            if (templateDTO.Criteria != null)
            {
                var existingCriteria = await _unitOfWork.LevelFeedbackCriterionRepository
                    .GetCriteriaByTemplateIdAsync(templateId);
                var existingIds = existingCriteria.Select(c => c.CriterionId).ToList();
                var updatedIds = templateDTO.Criteria
                    .Where(c => c.CriterionId > 0)
                    .Select(c => c.CriterionId)
                    .ToList();

                // Delete criteria not in the updated list
                foreach (var criterionId in existingIds)
                {
                    if (!updatedIds.Contains(criterionId))
                    {
                        await _unitOfWork.LevelFeedbackCriterionRepository.DeleteAsync(criterionId);
                    }
                }

                // Update or add criteria
                int order = 1;
                foreach (var criterionDTO in templateDTO.Criteria)
                {
                    if (criterionDTO.CriterionId > 0)
                    {
                        // Update existing criterion
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
                        // Add new criterion
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
                Message = "Feedback template updated successfully"
            };
        }

        public async Task<ResponseDTO> DeleteTemplateAsync(int templateId)
        {
            // Check if template exists
            var template = await _unitOfWork.LevelFeedbackTemplateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Template not found"
                };
            }

            // Check if template is used in any feedback
            var feedbacks = await _unitOfWork.ClassFeedbackRepository.GetAllAsync(f => f.TemplateId == templateId);
            if (feedbacks != null && feedbacks.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cannot delete template as it is used in existing feedback entries"
                };
            }

            // Delete all criteria first
            var criteria = await _unitOfWork.LevelFeedbackCriterionRepository.GetCriteriaByTemplateIdAsync(templateId);
            foreach (var criterion in criteria)
            {
                await _unitOfWork.LevelFeedbackCriterionRepository.DeleteAsync(criterion.CriterionId);
            }

            // Then delete template
            await _unitOfWork.LevelFeedbackTemplateRepository.DeleteAsync(templateId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback template deleted successfully"
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
                    Message = "Template not found"
                };
            }

            // Deactivate any other active templates for this level
            var activeTemplates = await _unitOfWork.LevelFeedbackTemplateRepository.GetAllAsync(
                t => t.LevelId == template.LevelId && t.IsActive && t.TemplateId != templateId);

            foreach (var activeTemplate in activeTemplates)
            {
                activeTemplate.IsActive = false;
                await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(activeTemplate);
            }

            // Activate this template
            template.IsActive = true;
            await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(template);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback template activated successfully"
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
                    Message = "Template not found"
                };
            }

            template.IsActive = false;
            await _unitOfWork.LevelFeedbackTemplateRepository.UpdateAsync(template);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback template deactivated successfully"
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
