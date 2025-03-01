using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.QnAReplies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QnARepliesController : ControllerBase
    {
        private readonly IQnARepliesService _qnaReplyService;

        public QnARepliesController(IQnARepliesService qnaReplyService)
        {
            _qnaReplyService = qnaReplyService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllQnAReplies()
        {
            var response = await _qnaReplyService.GetAllQnARepliesAsync();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQnARepliesById(int id)
        {
            var response = await _qnaReplyService.GetQnARepliesByIdAsync(id);
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddQnAReplies([FromBody] CreateQnARepliesDTO createDto)
        {
            var response = await _qnaReplyService.CreateQnARepliesAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateQnAReplies(int id, [FromBody] UpdateQnARepliesDTO updateDto)
        {
            var response = await _qnaReplyService.UpdateQnARepliesAsync(id, updateDto);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteQnAReplies(int id)
        {
            var response = await _qnaReplyService.DeleteQnARepliesAsync(id);
            return Ok(response);
        }
    }
}
