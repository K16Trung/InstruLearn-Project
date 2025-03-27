using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTeachers()
        {
            var result = await _teacherService.GetAllTeachersAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var result = await _teacherService.GetTeacherByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeacher(CreateTeacherDTO createTeacherDTO)
        {
            var result = await _teacherService.CreateTeacherAsync(createTeacherDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDTO updateTeacherDTO)
        {
            var result = await _teacherService.UpdateTeacherAsync(id, updateTeacherDTO);
            return Ok(result);
        }

        [HttpPut("ban/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var result = await _teacherService.DeleteTeacherAsync(id);
            return Ok(result);
        }

        [HttpPut("unban/{id}")]
        public async Task<IActionResult> UnbanTeacher(int id)
        {
            var result = await _teacherService.UnbanTeacherAsync(id);
            return Ok(result);
        }
    }
}
