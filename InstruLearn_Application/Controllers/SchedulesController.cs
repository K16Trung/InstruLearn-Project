using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO;
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
                return Ok(response);
            }

            return Ok(response);
        }
        
        [HttpGet("teacher/{teacherId}/classs")]
        public async Task<IActionResult> GetClassSchedulessByTeacher(int teacherId)
        {
            var response = await _scheduleService.GetClassSchedulesByTeacherIdAsyncc(teacherId);

            if (!response.IsSucceed)
            {
                return Ok(response);
            }

            return Ok(response);
        }

        [HttpGet("learner/{learnerId}/class")]
        public async Task<IActionResult> GetClassSchedulesByLearner(int learnerId)
        {
            var response = await _scheduleService.GetClassSchedulesByLearnerIdAsync(learnerId);

            if (!response.IsSucceed)
            {
                return Ok(response);
            }

            return Ok(response);
        }

        [HttpGet("available-teachers")]
        public async Task<IActionResult> GetAvailableTeachers([FromQuery] int majorId, [FromQuery] TimeOnly timeStart, [FromQuery] int timeLearning, [FromQuery] string startDay)
        {
            if (startDay == null || !startDay.Any())
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "At least one day must be specified for checking teacher availability."
                });
            }

            try
            {
                // Parse comma-separated date strings into DateOnly array
                string[] dateStrings = startDay.Split(',').Select(d => d.Trim()).ToArray();
                var startDays = new List<DateOnly>();

                foreach (var dateString in dateStrings)
                {
                    if (DateOnly.TryParse(dateString, out DateOnly date))
                    {
                        startDays.Add(date);
                    }
                    else
                    {
                        return BadRequest(new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Invalid date format: '{dateString}'. Expected format: yyyy-MM-dd"
                        });
                    }
                }

                if (!startDays.Any())
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No valid dates provided. Expected format: yyyy-MM-dd"
                    });
                }

                var result = await _scheduleService.GetAvailableTeachersAsync(majorId, timeStart, timeLearning, startDays.ToArray());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error processing date input: {ex.Message}"
                });
            }
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
        public async Task<IActionResult> UpdateAttendance(int scheduleId, [FromBody] UpdateAttendanceDTO model)
        {
            if (model == null)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid request data."
                });
            }

            var result = await _scheduleService.UpdateAttendanceAsync(
                scheduleId,
                model.Status,
                model.PreferenceStatus);

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

        [HttpGet("attendance-stats/learner/{learnerId}")]
        public async Task<IActionResult> GetLearnerAttendanceStats(int learnerId)
        {
            var result = await _scheduleService.GetLearnerAttendanceStatsAsync(learnerId);
            return Ok(result);
        }

        [HttpPut("update-teacher/{scheduleId}")]
        public async Task<IActionResult> UpdateScheduleTeacher(int scheduleId, [FromBody] UpdateScheduleTeacherDTO model)
        {
            if (model == null || model.TeacherId <= 0)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid teacher ID."
                });
            }

            var result = await _scheduleService.UpdateScheduleTeacherAsync(scheduleId, model.TeacherId, model.ChangeReason);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Add to InstruLearn_Application/Controllers/SchedulesController.cs
        [HttpPut("makeup/{scheduleId}")]
        public async Task<IActionResult> UpdateScheduleForMakeup(int scheduleId, [FromBody] UpdateScheduleMakeupDTO model)
        {
            if (model == null)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid request data."
                });
            }

            if (string.IsNullOrEmpty(model.ChangeReason))
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Change reason is required."
                });
            }

            if (model.TimeLearning <= 0)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Time learning must be greater than 0 minutes."
                });
            }

            var result = await _scheduleService.UpdateScheduleForMakeupAsync(scheduleId, model.NewDate, model.NewTimeStart, model.TimeLearning, model.ChangeReason);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("auto-update-attendance")]
        public async Task<IActionResult> AutoUpdateAttendance()
        {
            var result = await _scheduleService.AutoUpdateAttendanceStatusAsync();
            return Ok(result);
        }
    }
}
