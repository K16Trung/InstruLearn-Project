using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningRegisController : ControllerBase
    {
        private readonly ILearningRegisService _learningRegisService;

        public LearningRegisController(ILearningRegisService learningRegisService)
        {
            _learningRegisService = learningRegisService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllLearningRegis()
        {
            var response = await _learningRegisService.GetAllLearningRegisAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLearningRegisById(int id)
        {
            var response = await _learningRegisService.GetLearningRegisByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddLearningRegis([FromBody] CreateLearningRegisDTO createLearningRegisDTO)
        {
            var response = await _learningRegisService.CreateLearningRegisAsync(createLearningRegisDTO);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLearningRegis(int id)
        {
            var response = await _learningRegisService.DeleteLearningRegisAsync(id);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetAllPendingRegistrations()
        {
            var result = await _learningRegisService.GetAllPendingRegistrationsAsync();
            return Ok(result);
        }

        [HttpGet("status/{learnerId}")]
        public async Task<IActionResult> GetRegistrationsByLearnerId(int learnerId)
        {
            var result = await _learningRegisService.GetRegistrationsByLearnerIdAsync(learnerId);
            return Ok(result);
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateLearningRegisStatus([FromBody] UpdateLearningRegisDTO updateDTO)
        {
            var result = await _learningRegisService.UpdateLearningRegisStatusAsync(updateDTO);

            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

    }
}
