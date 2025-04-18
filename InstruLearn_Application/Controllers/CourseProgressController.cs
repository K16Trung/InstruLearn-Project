using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseProgressController : ControllerBase
    {
        private readonly ICourseProgressService _courseProgressService;

        public CourseProgressController(ICourseProgressService courseProgressService)
        {
            _courseProgressService = courseProgressService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCourseProgress(UpdateLearnerCourseProgressDTO updateDto)
        {
            var response = await _courseProgressService.UpdateCourseProgressAsync(updateDto);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpPost("update-content-item")]
        public async Task<IActionResult> UpdateContentItemProgress(int learnerId, int contentItemId, bool isCompleted)
        {
            var response = await _courseProgressService.UpdateContentItemProgressAsync(learnerId, contentItemId, isCompleted);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetCourseProgress(int learnerId, int coursePackageId)
        {
            var response = await _courseProgressService.GetCourseProgressAsync(learnerId, coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("learner/{learnerId}")]
        public async Task<IActionResult> GetAllCourseProgressByLearner(int learnerId)
        {
            var response = await _courseProgressService.GetAllCourseProgressByLearnerAsync(learnerId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("course/{coursePackageId}")]
        public async Task<IActionResult> GetAllLearnersForCourse(int coursePackageId)
        {
            var response = await _courseProgressService.GetAllLearnersForCourseAsync(coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }
    }
}