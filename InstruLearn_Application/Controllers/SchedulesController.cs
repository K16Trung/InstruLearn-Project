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

        /*[HttpGet("learningRegis/{learningRegisId}")]
        public async Task<IActionResult> GetSchedulesByLearningRegisId(int learningRegisId)
        {
            var schedules = await _scheduleService.GetSchedulesByLearningRegisIdAsync(learningRegisId);

            if (schedules == null || !schedules.Any())
            {
                return NotFound("No schedules found for this Learning Registration ID.");
            }

            return Ok(schedules);
        }*/
        
        [HttpGet("learningRegis/{learningRegisId}/schedules")]
        public async Task<IActionResult> GetSchedulesAsync(int learningRegisId)
        {
            var result = await _scheduleService.GetSchedulesAsync(learningRegisId);
            return Ok(result);
        }
        
        [HttpGet("learner/{learnerId}/schedules")]
        public async Task<IActionResult> GetSchedulesByLearnerAsync(int learnerId)
        {
            var result = await _scheduleService.GetSchedulesByLearnerIdAsync(learnerId);
            return Ok(result);
        }
        
        [HttpGet("teacher/{teacherId}/register")]
        public async Task<IActionResult> GetSchedulesByTeacherAsync(int teacherId)
        {
            var result = await _scheduleService.GetSchedulesByTeacherIdAsync(teacherId);
            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}/class")]
        public async Task<IActionResult> GetClassSchedulesByTeacher(int teacherId)
        {
            var response = await _scheduleService.GetClassSchedulesByTeacherIdAsync(teacherId);

            if (!response.IsSucceed)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        
        [HttpGet("teacher/{teacherId}/classs")]
        public async Task<IActionResult> GetClassSchedulessByTeacher(int teacherId)
        {
            var response = await _scheduleService.GetClassSchedulesByTeacherIdAsyncc(teacherId);

            if (!response.IsSucceed)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet("learner/{learnerId}/class")]
        public async Task<IActionResult> GetClassSchedulesByLearner(int learnerId)
        {
            var response = await _scheduleService.GetClassSchedulesByLearnerIdAsync(learnerId);

            if (!response.IsSucceed)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet("available-teachers")]
        public async Task<IActionResult> GetAvailableTeachers([FromQuery] int majorId, [FromQuery] TimeOnly timeStart, [FromQuery] int timeLearning, [FromQuery] DateOnly startDay)
        {
            var result = await _scheduleService.GetAvailableTeachersAsync(majorId, timeStart, timeLearning, startDay);
            return Ok(result);
        }

    }
}
