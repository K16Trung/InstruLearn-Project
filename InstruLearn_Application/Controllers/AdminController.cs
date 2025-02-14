using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAdmins()
        {
            var result = await _adminService.GetAllAdminAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            var result = await _adminService.GetAdminByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAdmin(CreateAdminDTO createAdminDTO)
        {
            var result = await _adminService.CreateAdminAsync(createAdminDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, UpdateAdminDTO updateAdminDTO)
        {
            var result = await _adminService.UpdateAdminAsync(id, updateAdminDTO);
            return Ok(result);
        }
    }
}
