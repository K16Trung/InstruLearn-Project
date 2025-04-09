using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.SyllbusContent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyllabusContentController : ControllerBase
    {
        private readonly ISyllabusContentService _syllabusContentService;
        public SyllabusContentController(ISyllabusContentService syllabusContentService)
        {
            _syllabusContentService = syllabusContentService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllSyllabusContents()
        {
            var result = await _syllabusContentService.GetAllSyllabusContentsAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSyllabusContentById(int id)
        {
            var result = await _syllabusContentService.GetSyllabusContentByIdAsync(id);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddSyllabusContent([FromBody] CreateSyllabusContentDTO createDto)
        {
            var response = await _syllabusContentService.AddSyllabusContentAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSyllabusContent(int id, [FromBody] UpdateSyllabusContentDTO updateDto)
        {
            var result = await _syllabusContentService.UpdateSyllabusContentAsync(id, updateDto);
            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSyllabusContent(int id)
        {
            var result = await _syllabusContentService.DeleteSyllabusContentAsync(id);
            return Ok(result);
        }
    }
}
