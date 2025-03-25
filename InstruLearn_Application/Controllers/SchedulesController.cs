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
    }
}
