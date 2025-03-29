using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Course;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCourses()
        {
            var response = await _courseService.GetAllCoursesAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var response = await _courseService.GetCourseByIdAsync(id);
            return Ok(response);
        }

        [HttpGet("status0")]
        public async Task<IActionResult> GetCoursesWithStatusZero()
        {
            var courses = await _courseService.GetAllCoursesWithStatusZeroAsync();
            return Ok(courses);
        }

        [HttpGet("status1")]
        public async Task<IActionResult> GetCoursesWithStatusOne()
        {
            var courses = await _courseService.GetAllCoursesWithStatusOneAsync();
            return Ok(courses);
        }


        [HttpPost("create")]
        public async Task<IActionResult> AddCourse([FromBody] CreateCourseDTO createDto)
        {
            var response = await _courseService.AddCourseAsync(createDto);
            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDTO updateDto)
        {
            var response = await _courseService.UpdateCourseAsync(id, updateDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var response = await _courseService.DeleteCourseAsync(id);
            return Ok(response);
        }
    }

}
