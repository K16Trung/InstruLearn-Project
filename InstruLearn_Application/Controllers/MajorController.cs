using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO.Major;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MajorController : ControllerBase
    {
        private readonly IMajorService _majorService;

        public MajorController(IMajorService majorService)
        {
            _majorService = majorService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllMajor()
        {
            var response = await _majorService.GetAllMajorAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMajorById(int id)
        {
            var response = await _majorService.GetMajorByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddMajor([FromBody] CreateMajorDTO createDto)
        {
            var response = await _majorService.AddMajorAsync(createDto);
            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMajor(int id, [FromBody] UpdateMajorDTO updateDto)
        {
            var response = await _majorService.UpdateMajorAsync(id, updateDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMajor(int id)
        {
            var response = await _majorService.DeleteMajorAsync(id);
            return Ok(response);
        }
    }
}
