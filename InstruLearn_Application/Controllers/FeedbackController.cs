using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllFeedback()
        {
            var response = await _feedbackService.GetAllFeedbackAsync();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var response = await _feedbackService.GetFeedbackByIdAsync(id);
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddFeedback([FromBody] CreateFeedbackDTO createDto)
        {
            var response = await _feedbackService.CreateFeedbackAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackDTO updateDto)
        {
            var response = await _feedbackService.UpdateFeedbackAsync(id, updateDto);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var response = await _feedbackService.DeleteFeedbackAsync(id);
            return Ok(response);
        }
    }
}
