using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LevelAssigned;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LevelAssignedController : ControllerBase
    {
        private readonly ILevelAssignedService _levelAssignedService;

        public LevelAssignedController(ILevelAssignedService levelAssignedService)
        {
            _levelAssignedService = levelAssignedService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllLevelAssigned()
        {
            var result = await _levelAssignedService.GetAllLevelAssigned();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLevelAssignedById(int id)
        {
            var result = await _levelAssignedService.GetLevelAssignedById(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateLevelAssigned(CreateLevelAssignedDTO createLevelAssignedDTO)
        {
            var result = await _levelAssignedService.CreateLevelAssigned(createLevelAssignedDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateLevelAssigned(int id, UpdateLevelAssignedDTO updateLevelAssignedDTO)
        {
            var result = await _levelAssignedService.UpdateLevelAssigned(id, updateLevelAssignedDTO);
            return Ok(result);
        }

        [HttpPut("update-syllabus-link/{id}")]
        public async Task<IActionResult> UpdateSyllabusLink(int id, UpdateSyllabusLinkDTO updateSyllabusLinkDTO)
        {
            var result = await _levelAssignedService.UpdateSyllabusLink(id, updateSyllabusLinkDTO);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLevelAssigned(int id)
        {
            var result = await _levelAssignedService.DeleteLevelAssigned(id);
            return Ok(result);
        }
    }
}