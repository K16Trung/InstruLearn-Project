using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Learner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerController : ControllerBase
    {
        private readonly ILearnerService _learnerService;

        public LearnerController(ILearnerService learnerService)
        {
            _learnerService = learnerService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllLearners()
        {
            var result = await _learnerService.GetAllLearnerAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLearnerById(int id)
        {
            var result = await _learnerService.GetLearnerByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateLearner(int id, UpdateLearnerDTO updateLearnerDTO)
        {
            var result = await _learnerService.UpdateLearnerAsync(id, updateLearnerDTO);
            return Ok(result);
        }

        [HttpPut("ban/{id}")]
        public async Task<IActionResult> DeleteLearner(int id)
        {
            var result = await _learnerService.DeleteLearnerAsync(id);
            return Ok(result);
        }

        [HttpPut("unban/{id}")]
        public async Task<IActionResult> UnbanLearner(int id)
        {
            var result = await _learnerService.UnbanLearnerAsync(id);
            return Ok(result);
        }
    }
}
