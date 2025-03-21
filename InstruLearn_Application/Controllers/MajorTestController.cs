using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.MajorTest;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MajorTestController : ControllerBase
    {
        private readonly IMajorTestService _majorTestService;

        public MajorTestController(IMajorTestService majorTestService)
        {
            _majorTestService = majorTestService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllMajorTest()
        {
            var response = await _majorTestService.GetAllMajorTestsAsync();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMajorTestById(int id)
        {
            var response = await _majorTestService.GetMajorTestByIdAsync(id);
            return Ok(response);
        }
        [HttpGet("by-major/{id}")]
        public async Task<IActionResult> GetMajorTestsByMajorId(int id)
        {
            var response = await _majorTestService.GetMajorTestsByMajorIdAsync(id);
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddMajorTest([FromBody] CreateMajorTestDTO createDto)
        {
            var response = await _majorTestService.CreateMajorTestAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMajorTest(int id, [FromBody] UpdateMajorTestDTO updateDto)
        {
            var response = await _majorTestService.UpdateMajorTestAsync(id, updateDto);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMajorTest(int id)
        {
            var response = await _majorTestService.DeleteMajorTestAsync(id);
            return Ok(response);
        }
    }
}