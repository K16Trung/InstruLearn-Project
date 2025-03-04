using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.CenterCourse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CenterCourseController : ControllerBase
    {
        private readonly ICenterCourseService _centerCourseService;

        public CenterCourseController(ICenterCourseService centerCourseService)
        {
            _centerCourseService = centerCourseService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCenterCourse()
        {
            var centerCourse = await _centerCourseService.GetAllCenterCourseAsync();
            return Ok(centerCourse);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCenterCourseById(int id)
        {
            var centerCourse = await _centerCourseService.GetCenterCourseByIdAsync(id);
            return Ok(centerCourse);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddCenterCourse([FromBody] CreateCenterCourseDTO createCenterCourseDTO)
        {
            var response = await _centerCourseService.AddCenterCourseAsync(createCenterCourseDTO);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCenterCourse(int id, [FromBody] UpdateCenterCourseDTO updateCenterCourseDTO)
        {
            var response = await _centerCourseService.UpdateCenterCourseAsync(id, updateCenterCourseDTO);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCenterCourse(int id)
        {
            var response = await _centerCourseService.DeleteCenterCourseAsync(id);
            return Ok(response);
        }
    }
}
