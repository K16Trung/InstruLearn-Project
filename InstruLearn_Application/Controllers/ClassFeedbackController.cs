using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.ClassFeedback;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassFeedbackController : ControllerBase
    {
        private readonly IClassFeedbackService _feedbackService;

        public ClassFeedbackController(IClassFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet("GetFeedback/{feedbackId}")]
        public async Task<IActionResult> GetFeedback(int feedbackId)
        {
            var feedback = await _feedbackService.GetFeedbackAsync(feedbackId);

            if (feedback == null)
                return NotFound(new ResponseDTO { IsSucceed = false, Message = "Feedback not found" });

            return Ok(feedback);
        }

        [HttpGet("GetFeedbacksByClass/{classId}")]
        public async Task<IActionResult> GetFeedbacksByClass(int classId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByClassIdAsync(classId);
            return Ok(feedbacks);
        }

        [HttpGet("GetFeedbacksByLearner/{learnerId}")]
        public async Task<IActionResult> GetFeedbacksByLearner(int learnerId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByLearnerIdAsync(learnerId);
            return Ok(feedbacks);
        }

        [HttpGet("class/{classId}/learner/{learnerId}")]
        public async Task<IActionResult> GetFeedbackByClassAndLearner(int classId, int learnerId)
        {
            var feedback = await _feedbackService.GetFeedbackByClassAndLearnerAsync(classId, learnerId);

            if (feedback == null)
                return NotFound(new ResponseDTO { IsSucceed = false, Message = "Feedback not found" });

            return Ok(feedback);
        }

        [HttpGet("summary/class/{classId}")]
        public async Task<IActionResult> GetFeedbackSummaryForClass(int classId)
        {
            var summary = await _feedbackService.GetFeedbackSummaryForClassAsync(classId);

            if (summary == null)
                return NotFound(new ResponseDTO { IsSucceed = false, Message = "Class not found" });

            return Ok(summary);
        }

        [HttpPost("CreateFeedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateClassFeedbackDTO feedbackDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _feedbackService.CreateFeedbackAsync(feedbackDTO);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("UpdateFeedback/{feedbackId}")]
        public async Task<IActionResult> UpdateFeedback(int feedbackId, [FromBody] UpdateClassFeedbackDTO feedbackDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _feedbackService.UpdateFeedbackAsync(feedbackId, feedbackDTO);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("DeleteFeedback/{feedbackId}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            var response = await _feedbackService.DeleteFeedbackAsync(feedbackId);

            if (!response.IsSucceed)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
