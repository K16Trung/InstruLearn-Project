using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Staff;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllStaffs()
        {
            var result = await _staffService.GetAllStaffAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var result = await _staffService.GetStaffByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateStaff(CreateStaffDTO createStaffDTO)
        {
            var result = await _staffService.CreateStaffAsync(createStaffDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateStaff(int id, UpdateStaffDTO updateStaffDTO)
        {
            var result = await _staffService.UpdateStaffAsync(id, updateStaffDTO);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var result = await _staffService.DeleteStaffAsync(id);
            return Ok(result);
        }

        [HttpDelete("unban/{id}")]
        public async Task<IActionResult> UnbanStaff(int id)
        {
            var result = await _staffService.UnbanStaffAsync(id);
            return Ok(result);
        }
    }
}
