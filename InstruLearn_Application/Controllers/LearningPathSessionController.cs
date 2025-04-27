using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningPathSessionController : ControllerBase
    {
        private readonly ILearningPathService _learningPathService;

        public LearningPathSessionController(ILearningPathService learningPathService)
        {
            _learningPathService = learningPathService;
        }

        [HttpGet("{learningRegisId}/learning-path-sessions")]
        public async Task<IActionResult> GetLearningPathSessions(int learningRegisId)
        {
            var response = await _learningPathService.GetLearningPathSessionsAsync(learningRegisId);
            if (response.IsSucceed)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPut("update-learning-path-session")]
        public async Task<IActionResult> UpdateLearningPathSession([FromBody] UpdateLearningPathSessionDTO updateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _learningPathService.UpdateLearningPathSessionAsync(updateDTO);

            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("confirm-learning-path/{learningRegisId}")]
        public async Task<IActionResult> ConfirmLearningPath(int learningRegisId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _learningPathService.ConfirmLearningPathAsync(learningRegisId);

            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
