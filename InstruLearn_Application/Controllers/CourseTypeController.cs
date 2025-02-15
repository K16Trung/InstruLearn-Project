using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseTypeController : ControllerBase
    {
        private readonly ICourseTypeService _courseTypeService;

        public CourseTypeController(ICourseTypeService courseTypeService)
        {
            _courseTypeService = courseTypeService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCourseType()
        {
            var response = await _courseTypeService.GetAllCourseTypeAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseTypeById(int id)
        {
            var response = await _courseTypeService.GetCourseTypeByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddCourseType([FromBody] CreateCourseTypeDTO createDto)
        {
            var response = await _courseTypeService.AddCourseTypeAsync(createDto);
            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourseType(int id, [FromBody] UpdateCourseTypeDTO updateDto)
        {
            var response = await _courseTypeService.UpdateCourseTypeAsync(id, updateDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCourseType(int id)
        {
            var response = await _courseTypeService.DeleteCourseTypeAsync(id);
            return Ok(response);
        }
    }
}
