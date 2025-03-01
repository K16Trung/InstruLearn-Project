using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.FeedbackReplies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackRepliesController : ControllerBase
    {
        private readonly IFeedbackRepliesService _feedbackRepliesService;
        public FeedbackRepliesController(IFeedbackRepliesService feedbackRepliesService)
        {
            _feedbackRepliesService = feedbackRepliesService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllFeedbackReplies()
        {
            var response = await _feedbackRepliesService.GetAllFeedbackRepliesAsync();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackRepliesById(int id)
        {
            var response = await _feedbackRepliesService.GetFeedbackRepliesByIdAsync(id);
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddFeedbackReplies([FromBody] CreateFeedbackRepliesDTO createDto)
        {
            var response = await _feedbackRepliesService.CreateFeedbackRepliesAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFeedbackReplies(int id, [FromBody] UpdateFeedbackRepliesDTO updateDto)
        {
            var response = await _feedbackRepliesService.UpdateFeedbackRepliesAsync(id, updateDto);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFeedbackReplies(int id)
        {
            var response = await _feedbackRepliesService.DeleteFeedbackRepliesAsync(id);
            return Ok(response);
        }
    }
}
