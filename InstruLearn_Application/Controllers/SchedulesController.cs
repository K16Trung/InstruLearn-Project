using InstruLearn_Application.BLL.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("learningRegis/{learningRegisId}")]
        public async Task<IActionResult> GetSchedulesByLearningRegisId(int learningRegisId)
        {
            var schedules = await _scheduleService.GetSchedulesByLearningRegisIdAsync(learningRegisId);

            if (schedules == null || !schedules.Any())
            {
                return NotFound("No schedules found for this Learning Registration ID.");
            }

            return Ok(schedules);
        }
        
        [HttpGet("learningRegis/{learningRegisId}/schedules")]
        public async Task<IActionResult> GetSchedulesAsync(int learningRegisId)
        {
            var result = await _scheduleService.GetSchedulesAsync(learningRegisId);
            return Ok(result);
        }
        
        [HttpGet("learner/{learnerId}/schedules")]
        public async Task<IActionResult> GetSchedulesByLearnerAsync(int learnerId)
        {
            var result = await _scheduleService.GetSchedulesAsync(learnerId);
            return Ok(result);
        }
        
        [HttpGet("teacher/{teacherId}/schedules")]
        public async Task<IActionResult> GetSchedulesByTeacherAsync(int teacherId)
        {
            var result = await _scheduleService.GetSchedulesAsync(teacherId);
            return Ok(result);
        }
    }
}
