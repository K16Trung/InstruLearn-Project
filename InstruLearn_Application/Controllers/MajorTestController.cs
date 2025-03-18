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

        [HttpGet]
        public async Task<IActionResult> GetAllMajorTests()
        {
            var result = await _majorTestService.GetAllMajorTestsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMajorTestById(int id)
        {
            var result = await _majorTestService.GetMajorTestByIdAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("by-major/{majorId}")]
        public async Task<IActionResult> GetMajorTestsByMajorId(int majorId)
        {
            var result = await _majorTestService.GetMajorTestsByMajorIdAsync(majorId);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMajorTest([FromBody] CreateMajorTestDTO createMajorTestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _majorTestService.CreateMajorTestAsync(createMajorTestDTO);
            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetMajorTestById), new { id = ((MajorTestDTO)result.Data).MajorTestId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMajorTest(int id, [FromBody] UpdateMajorTestDTO updateMajorTestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _majorTestService.UpdateMajorTestAsync(id, updateMajorTestDTO);
            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMajorTest(int id)
        {
            var result = await _majorTestService.DeleteMajorTestAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}