using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private readonly IResponseService _responseService;

        public ResponseController(IResponseService responseService)
        {
            _responseService = responseService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllResponses()
        {
            var result = await _responseService.GetAllResponseAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResponseById(int id)
        {
            var result = await _responseService.GetResponseByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateResponse(CreateResponseDTO createResponseDTO)
        {
            var result = await _responseService.CreateResponseAsync(createResponseDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateResponse(int id, UpdateResponseDTO updateResponseDTO)
        {
            var result = await _responseService.UpdateResponseAsync(id, updateResponseDTO);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteResponse(int id)
        {
            var result = await _responseService.DeleteResponseAsync(id);
            return Ok(result);
        }
    }
}