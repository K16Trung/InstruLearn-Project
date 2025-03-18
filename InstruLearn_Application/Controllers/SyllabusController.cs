using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly ISyllabusService _syllabusService;

        public SyllabusController(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllSyllabus()
        {
            var result = await _syllabusService.GetAllSyllabusAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSyllabusById(int id)
        {
            var result = await _syllabusService.GetSyllabusByIdAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSyllabus([FromBody] CreateSyllabusDTO createSyllabusDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _syllabusService.CreateSyllabusAsync(createSyllabusDTO);
            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetSyllabusById), new { id = ((SyllabusDTO)result.Data).SyllabusId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSyllabus(int id, [FromBody] UpdateSyllabusDTO updateSyllabusDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _syllabusService.UpdateSyllabusAsync(id, updateSyllabusDTO);
            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSyllabus(int id)
        {
            var result = await _syllabusService.DeleteSyllabusAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}