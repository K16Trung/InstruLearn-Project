using InstruLearn_Application.Model.Models.DTO.LevelFeedbackTemplate;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILevelFeedbackTemplateService
    {
        Task<ResponseDTO> CreateTemplateAsync(CreateLevelFeedbackTemplateDTO templateDTO);
        Task<ResponseDTO> UpdateTemplateAsync(int templateId, UpdateLevelFeedbackTemplateDTO templateDTO);
        Task<ResponseDTO> DeleteTemplateAsync(int templateId);
        Task<ResponseDTO> ActivateTemplateAsync(int templateId);
        Task<ResponseDTO> DeactivateTemplateAsync(int templateId);
        Task<LevelFeedbackTemplateDTO> GetTemplateAsync(int templateId);
        Task<List<LevelFeedbackTemplateDTO>> GetAllTemplatesAsync();
        Task<LevelFeedbackTemplateDTO> GetTemplateForLevelAsync(int levelId);
    }
}
