using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        public ClassController(IClassService classService)
        {
            _classService = classService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllClassAsync()
        {
            var result = await _classService.GetAllClassAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassByIdAsync(int id)
        {
            var result = await _classService.GetClassByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetClassesByTeacherId(int teacherId)
        {
            var result = await _classService.GetClassesByTeacherIdAsync(teacherId);

            if (!result.IsSucceed)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("MajorId/{majorId}/class")]
        public async Task<IActionResult> GetClassesByCoursePackageId(int majorId)
        {
            var result = await _classService.GetClassesByMajorIdAsync(majorId);

            if (!result.IsSucceed)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddClassAsync([FromBody] CreateClassDTO createClassDTO)
        {
            var result = await _classService.AddClassAsync(createClassDTO);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateClassAsync(int id, [FromBody] UpdateClassDTO updateClassDTO)
        {
            var result = await _classService.UpdateClassAsync(id, updateClassDTO);
            return Ok(result);
        }

        [HttpPut("change-class")]
        public async Task<IActionResult> ChangeClassForLearner([FromBody] ChangeClassDTO changeClassDTO)
        {
            var result = await _classService.ChangeClassForLearnerAsync(changeClassDTO);

            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteClassAsync(int id)
        {
            var result = await _classService.DeleteClassAsync(id);
            return Ok(result);
        }

        [HttpPost("update-learner-eligibility")]
        public async Task<IActionResult> UpdateLearnerClassEligibility([FromBody] LearnerClassEligibilityDTO eligibilityDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _classService.UpdateLearnerClassEligibilityAsync(eligibilityDTO);

            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
