using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Certification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificationController : ControllerBase
    {
        private readonly ICertificationService _certificationService;

        public CertificationController(ICertificationService certificationService)
        {
            _certificationService = certificationService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCertification()
        {
            var response = await _certificationService.GetAllCertificationAsync();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificationById(int id)
        {
            var response = await _certificationService.GetCertificationByIdAsync(id);
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddCertificationback([FromBody] CreateCertificationDTO createDto)
        {
            var response = await _certificationService.CreateCertificationAsync(createDto);
            return Ok(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCertification(int id, [FromBody] UpdateCertificationDTO updateDto)
        {
            var response = await _certificationService.UpdateCertificationAsync(id, updateDto);
            return Ok(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            var response = await _certificationService.DeleteCertificationAsync(id);
            return Ok(response);
        }
    }
}
