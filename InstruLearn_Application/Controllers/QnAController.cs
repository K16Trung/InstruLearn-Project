using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.QnA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QnAController : ControllerBase
    {
        private readonly IQnAService _qnAService;

        public QnAController(IQnAService qnAService)
        {
            _qnAService = qnAService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GellAllQnA()
        {
            var response = await _qnAService.GetAllQnAAsync();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQnAById(int id)
        {
            var response = await _qnAService.GetQnAByIdAsync(id);
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddQnA([FromBody] CreateQnADTO createDto)
        {
            var response = await _qnAService.CreateQnAAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateQnA(int id, [FromBody] UpdateQnADTO updateDto)
        {
            var response = await _qnAService.UpdateQnAAsync(id, updateDto);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteQnA(int id)
        {
            var response = await _qnAService.DeleteQnAAsync(id);
            return Ok(response);
        }
    }
}
