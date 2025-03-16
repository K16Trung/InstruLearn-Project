using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningRegisTypeController : ControllerBase
    {
        private readonly ILearningRegisTypeService _learningRegisTypeService;

        public LearningRegisTypeController(ILearningRegisTypeService learningRegisTypeService)
        {
            _learningRegisTypeService = learningRegisTypeService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllLearningRegisType()
        {
            var response = await _learningRegisTypeService.GetAllLearningRegisTypeAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLearningRegisTypeById(int id)
        {
            var response = await _learningRegisTypeService.GetLearningRegisTypeByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddLearningRegis([FromBody] CreateRegisTypeDTO createTypeDTO)
        {
            var response = await _learningRegisTypeService.CreateLearningRegisTypeAsync(createTypeDTO);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLearningRegis(int id)
        {
            var response = await _learningRegisTypeService.DeleteLearningRegisTypeAsync(id);
            return Ok(response);
        }
    }
}
