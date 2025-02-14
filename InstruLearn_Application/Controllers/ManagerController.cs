using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllManagers()
        {
            var result = await _managerService.GetAllManagerAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetManagerById(int id)
        {
            var result = await _managerService.GetManagerByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateManager(CreateManagerDTO createManagerDTO)
        {
            var result = await _managerService.CreateManagerAsync(createManagerDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateManager(int id, UpdateManagerDTO updateManagerDTO)
        {
            var result = await _managerService.UpdateManagerAsync(id, updateManagerDTO);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            var result = await _managerService.DeleteManagerAsync(id);
            return Ok(result);
        }

        [HttpDelete("unban/{id}")]
        public async Task<IActionResult> UnbanManager(int id)
        {
            var result = await _managerService.UnbanManagerAsync(id);
            return Ok(result);
        }
    }
}
