using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LevelFeedbackTemplate;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LevelFeedbackTemplateController : ControllerBase
    {
        private readonly ILevelFeedbackTemplateService _templateService;

        public LevelFeedbackTemplateController(ILevelFeedbackTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet("GetAllTemplates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _templateService.GetAllTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("level/{levelId}")]
        public async Task<IActionResult> GetTemplateForLevel(int levelId)
        {
            var template = await _templateService.GetTemplateForLevelAsync(levelId);

            if (template == null)
                return NotFound(new ResponseDTO { IsSucceed = false, Message = "No active template found for this level" });

            return Ok(template);
        }

        [HttpPost("CreateTemplate")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateLevelFeedbackTemplateDTO templateDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _templateService.CreateTemplateAsync(templateDTO);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("UpdateTemplate/{templateId}")]
        public async Task<IActionResult> UpdateTemplate(int templateId, [FromBody] UpdateLevelFeedbackTemplateDTO templateDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _templateService.UpdateTemplateAsync(templateId, templateDTO);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("DeleteTemplate/{templateId}")]
        public async Task<IActionResult> DeleteTemplate(int templateId)
        {
            var response = await _templateService.DeleteTemplateAsync(templateId);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{templateId}/activate")]
        public async Task<IActionResult> ActivateTemplate(int templateId)
        {
            var response = await _templateService.ActivateTemplateAsync(templateId);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{templateId}/deactivate")]
        public async Task<IActionResult> DeactivateTemplate(int templateId)
        {
            var response = await _templateService.DeactivateTemplateAsync(templateId);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("GetTemplate/{templateId}")]
        public async Task<IActionResult> GetTemplate(int templateId)
        {
            var template = await _templateService.GetTemplateAsync(templateId);

            if (template == null)
                return NotFound(new ResponseDTO { IsSucceed = false, Message = "Template not found" });

            return Ok(template);
        }

        
    }
}
