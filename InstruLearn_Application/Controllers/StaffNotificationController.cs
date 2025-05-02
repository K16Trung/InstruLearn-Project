using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.StaffNotification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffNotificationController : ControllerBase
    {
        private readonly IStaffNotificationService _staffNotificationService;

        public StaffNotificationController(IStaffNotificationService staffNotificationService)
        {
            _staffNotificationService = staffNotificationService;
        }

        [HttpGet("teacher-change-requests")]
        public async Task<ActionResult<ResponseDTO>> GetTeacherChangeRequests()
        {
            var result = await _staffNotificationService.GetAllTeacherChangeRequestsAsync();

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("GetLearningRegis-ChangeTeacher")]
        public async Task<ActionResult<ResponseDTO>> GetTeacherChangeLearningRegistrations()
        {
            var result = await _staffNotificationService.GetTeacherChangeRequestLearningRegistrationsAsync();

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPut("mark-as-read/{notificationId}")]
        public async Task<ActionResult<ResponseDTO>> MarkAsRead(int notificationId)
        {
            var result = await _staffNotificationService.MarkNotificationAsReadAsync(notificationId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("mark-as-resolved/{notificationId}")]
        public async Task<ActionResult<ResponseDTO>> MarkAsResolved(int notificationId)
        {
            var result = await _staffNotificationService.MarkNotificationAsResolvedAsync(notificationId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("changeTeacher")]
        public async Task<IActionResult> ChangeTeacher([FromBody] ChangeTeacherRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _staffNotificationService.ChangeTeacherForLearningRegistrationAsync(
                request.NotificationId,
                request.LearningRegisId,
                request.NewTeacherId,
                request.ChangeReason);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

    }
}
