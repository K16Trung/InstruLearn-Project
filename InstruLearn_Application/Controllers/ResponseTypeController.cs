using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.ResponseType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseTypeController : ControllerBase
    {
        private readonly IResponseTypeService _responseTypeService;

        public ResponseTypeController(IResponseTypeService responseTypeService)
        {
            _responseTypeService = responseTypeService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllResponseTypes()
        {
            var result = await _responseTypeService.GetAllResponseType();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResponseTypeById(int id)
        {
            var result = await _responseTypeService.GetResponseTypeById(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateResponseType(CreateResponseTypeDTO createResponseTypeDTO)
        {
            var result = await _responseTypeService.CreateResponseType(createResponseTypeDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateResponseType(int id, UpdateResponseTypeDTO updateResponseTypeDTO)
        {
            var result = await _responseTypeService.UpdateResponseType(id, updateResponseTypeDTO);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteResponseType(int id)
        {
            var result = await _responseTypeService.DeleteResponseType(id);
            return Ok(result);
        }
    }
}