using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        public ClassController(IClassService classService)
        {
            _classService = classService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllClassAsync()
        {
            var result = await _classService.GetAllClassAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassByIdAsync(int classId)
        {
            var result = await _classService.GetClassByIdAsync(classId);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddClassAsync([FromBody] CreateClassDTO createClassDTO)
        {
            var result = await _classService.AddClassAsync(createClassDTO);
            return Ok(result);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateClassAsync(int classId, [FromBody] UpdateClassDTO updateClassDTO)
        {
            var result = await _classService.UpdateClassAsync(classId, updateClassDTO);
            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteClassAsync(int classId)
        {
            var result = await _classService.DeleteClassAsync(classId);
            return Ok(result);
        }
    }
}
