using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Enum;
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

        [HttpGet("class-attendance/{classId}")]
        public async Task<IActionResult> GetClassAttendance(int classId)
        {
            var result = await _scheduleService.GetClassAttendanceAsync(classId);
            return Ok(result);
        }

        [HttpGet("one-on-one-attendance/{learnerId}")]
        public async Task<IActionResult> GetOneOnOneAttendance(int learnerId)
        {
            var result = await _scheduleService.GetOneOnOneAttendanceAsync(learnerId);
            return Ok(result);
        }

        [HttpPut("update-attendance/{scheduleId}")]
        public async Task<IActionResult> UpdateAttendance(int scheduleId, [FromBody] AttendanceStatus status)
        {
            var result = await _scheduleService.UpdateAttendanceAsync(scheduleId, status);
            return Ok(result);
        }

        [HttpGet("conflict-check/learner/{learnerId}")]
        public async Task<IActionResult> CheckLearnerScheduleConflict(int learnerId, [FromQuery] DateOnly startDay, [FromQuery] TimeOnly timeStart, [FromQuery] int durationMinutes)
        {
            var result = await _scheduleService.CheckLearnerScheduleConflictAsync(
                learnerId, startDay, timeStart, durationMinutes);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("conflict-check/learner/{learnerId}/class/{classId}")]
        public async Task<IActionResult> CheckLearnerClassScheduleConflict(int learnerId, int classId)
        {
            var result = await _scheduleService.CheckLearnerClassScheduleConflictAsync(learnerId, classId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }



    }
}
