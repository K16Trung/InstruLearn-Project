using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.TeacherMajor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherMajorController : ControllerBase
    {
        private readonly ITeacherMajorService _teacherMajorService;
        public TeacherMajorController(ITeacherMajorService teacherMajorService)
        {
            _teacherMajorService = teacherMajorService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTeacherMajorAsync()
        {
            var response = await _teacherMajorService.GetAllTeacherMajorAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherMajorByIdAsync(int id)
        {
            var response = await _teacherMajorService.GetTeacherMajorByIdAsync(id);
            return Ok(response);
        }

        [HttpPut("update/{id}/Busy")]
        public async Task<IActionResult> UpdateBusyTeacherMajorAsync(int id)
        {
            var response = await _teacherMajorService.UpdateBusyStatusTeacherMajorAsync(id);
            return Ok(response);
        }
        [HttpPut("update/{id}/Free")]
        public async Task<IActionResult> UpdateFreeTeacherMajorAsync(int id)
        {
            var response = await _teacherMajorService.UpdateFreeStatusTeacherMajorAsync(id);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTeacherMajorAsync(int id)
        {
            var response = await _teacherMajorService.DeleteTeacherMajorAsync(id);
            return Ok(response);
        }
    }
}
