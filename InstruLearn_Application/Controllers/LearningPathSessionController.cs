using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
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
    }
}
