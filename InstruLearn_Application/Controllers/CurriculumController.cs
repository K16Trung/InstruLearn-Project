using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Curriculum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurriculumController : ControllerBase
    {
        private readonly ICurriculumService _curriculumService;

        public CurriculumController(ICurriculumService curriculumService)
        {
            _curriculumService = curriculumService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCurriculum()
        {
            var result = await _curriculumService.GetAllCurriculumAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCurriculumById(int id)
        {
            var result = await _curriculumService.GetCurriculumByIdAsync(id);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateCurriculum([FromBody] CreateCurriculumDTO createCurriculumDTO)
        {
            var result = await _curriculumService.AddCurriculumAsync(createCurriculumDTO);
            return Ok(result);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCurriculum(int id, [FromBody] UpdateCurriculumDTO updateCurriculumDTO)
        {
            var result = await _curriculumService.UpdateCurriculumAsync(id, updateCurriculumDTO);
            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCurriculum(int id)
        {
            var result = await _curriculumService.DeleteCurriculumAsync(id);
            return Ok(result);
        }
    }
}
