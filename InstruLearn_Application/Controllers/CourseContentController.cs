using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Course_Content;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseContentController : ControllerBase
    {
        private readonly ICourseContentService _courseContentService;
        private readonly ICourseProgressService _courseProgressService;

        public CourseContentController(ICourseContentService courseContentService,
                                       ICourseProgressService courseProgressService)
        {
            _courseContentService = courseContentService;
            _courseProgressService = courseProgressService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCourseContent()
        {
            var response = await _courseContentService.GetAllCourseContentAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseContentById(int id)
        {
            var response = await _courseContentService.GetCourseContentByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddCourseContent([FromBody] CreateCourseContentDTO createDto)
        {
            var response = await _courseContentService.AddCourseContentAsync(createDto);

            if (response.IsSucceed)
            {
                await _courseProgressService.RecalculateAllLearnersProgressForCourse(createDto.CoursePackageId);
            }

            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourseContent(int id, [FromBody] UpdateCourseContentDTO updateDto)
        {
            var response = await _courseContentService.UpdateCourseContentAsync(id, updateDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCourseContent(int id)
        {
            var response = await _courseContentService.DeleteCourseContentAsync(id);
            return Ok(response);
        }
    }
}
