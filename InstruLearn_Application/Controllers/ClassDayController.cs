using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassDayController : ControllerBase
    {
        private readonly IClassDayService _classDayService;

        public ClassDayController(IClassDayService classDayService)
        {
            _classDayService = classDayService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllClassDay()
        {
            var result = await _classDayService.GetAllClassDayAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassDayById(int id)
        {
            var result = await _classDayService.GetClassDayByIdAsync(id);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddClassDay([FromBody] CreateClassDayDTO createClassDayDTO)
        {
            var result = await _classDayService.AddClassDayAsync(createClassDayDTO);
            return Ok(result);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateClassDay(int id, [FromBody] UpdateClassDayDTO updateClassDayDTO)
        {
            var result = await _classDayService.UpdateClassDayAsync(id, updateClassDayDTO);
            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteClassDay(int id)
        {
            var result = await _classDayService.DeleteClassDayAsync(id);
            return Ok(result);
        }
    }
}
