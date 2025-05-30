using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
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
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly ILogger<CertificationController> _logger;

        public CertificationController(ICertificationService certificationService, IGoogleSheetsService googleSheetsService, ILogger<CertificationController> logger)
        {
            _certificationService = certificationService;
            _googleSheetsService = googleSheetsService;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCertificates()
        {
            try
            {
                _logger.LogInformation("Received request to get all certificates");

                var certificates = await _googleSheetsService.GetAllCertificatesAsync();

                return Ok(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Successfully retrieved {certificates.Count} certificates",
                    Data = certificates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all certificates");

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to retrieve certificates: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificationById(int id)
        {
            var response = await _certificationService.GetCertificationByIdAsync(id);
            return Ok(response);
        }
        
        [HttpGet("GetByLearnerId/{learnerId}")]
        public async Task<IActionResult> GetByLearnerId(int learnerId)
        {
            var result = await _certificationService.GetLearnerCertificationsAsync(learnerId);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddCertificationback([FromBody] CreateCertificationDTO createDto)
        {
            try
            {
                var response = await _certificationService.CreateCertificationAsync(createDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error creating certificate: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCertification(int id, [FromBody] UpdateCertificationDTO updateDto)
        {
            var response = await _certificationService.UpdateCertificationAsync(id, updateDto);
            return Ok(response);
        }

        [HttpGet("test-google-sheets-connection")]
        public async Task<IActionResult> TestGoogleSheetsConnection()
        {
            try
            {
                var googleSheetsService = HttpContext.RequestServices.GetRequiredService<IGoogleSheetsService>();
                var result = await googleSheetsService.TestGoogleSheetsConnectionAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error testing Google Sheets connection: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            var response = await _certificationService.DeleteCertificationAsync(id);
            return Ok(response);
        }
    }
}
