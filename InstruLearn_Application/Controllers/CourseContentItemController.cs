using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Course_Content;
using InstruLearn_Application.Model.Models.DTO.Course_Content_Item;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseContentItemController : ControllerBase
    {
        private readonly ICourseContentItemService _courseContentItemService;

        public CourseContentItemController(ICourseContentItemService courseContentItemService)
        {
            _courseContentItemService = courseContentItemService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCourseContentItem()
        {
            var response = await _courseContentItemService.GetAllCourseContentItemsAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseContentItemById(int id)
        {
            var response = await _courseContentItemService.GetCourseContentItemByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddCourseContentItem([FromBody] CreateCourseContentItemDTO createDto)
        {
            var response = await _courseContentItemService.AddCourseContentItemAsync(createDto);
            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourseContentItem(int id, [FromBody] UpdateCourseContentItemDTO updateDto)
        {
            var response = await _courseContentItemService.UpdateCourseContentItemAsync(id, updateDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCourseContentItem(int id)
        {
            var response = await _courseContentItemService.DeleteCourseContentItemAsync(id);
            return Ok(response);
        }
    }
}
