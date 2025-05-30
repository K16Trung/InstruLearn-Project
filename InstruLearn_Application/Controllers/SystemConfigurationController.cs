using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.SystemConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly ISystemConfigurationService _configurationService;

        public SystemConfigurationController(ISystemConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConfigurations()
        {
            var response = await _configurationService.GetAllConfigurationsAsync();
            return Ok(response);
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetConfiguration(string key)
        {
            var response = await _configurationService.GetConfigurationAsync(key);
            if (response.IsSucceed)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateConfiguration(string key, [FromBody] UpdateConfigurationDTO updateDTO)
        {
            if (string.IsNullOrWhiteSpace(updateDTO.Value))
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Value cannot be empty."
                });
            }

            var response = await _configurationService.UpdateConfigurationAsync(key, updateDTO.Value, updateDTO.Description);
            if (response.IsSucceed)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}