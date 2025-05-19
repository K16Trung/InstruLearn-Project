using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.SelfAssessment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfAssessmentController : ControllerBase
    {
        private readonly ISelfAssessmentService _selfAssessmentService;

        public SelfAssessmentController(ISelfAssessmentService selfAssessmentService)
        {
            _selfAssessmentService = selfAssessmentService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _selfAssessmentService.GetAllAsync();
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _selfAssessmentService.GetByIdAsync(id);
            return response.IsSucceed ? Ok(response) : Ok(response);
        }

        [HttpGet("GetByIdWithRegistrations/{id}")]
        public async Task<IActionResult> GetByIdWithRegistrations(int id)
        {
            var response = await _selfAssessmentService.GetByIdWithRegistrationsAsync(id);
            return response.IsSucceed ? Ok(response) : Ok(response);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateSelfAssessmentDTO createDTO)
        {
            var response = await _selfAssessmentService.CreateAsync(createDTO);
            return response.IsSucceed
                ? CreatedAtAction(nameof(GetById), new { id = ((SelfAssessmentDTO)response.Data).SelfAssessmentId }, response)
                : BadRequest(response);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateSelfAssessmentDTO updateDTO)
        {
            var response = await _selfAssessmentService.UpdateAsync(id, updateDTO);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("/Delete{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _selfAssessmentService.DeleteAsync(id);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }
    }
}
