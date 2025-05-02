using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer;
using InstruLearn_Application.Model.Models.DTO.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackNotificationController : ControllerBase
    {
        private readonly IFeedbackNotificationService _feedbackNotificationService;

        public FeedbackNotificationController(IFeedbackNotificationService feedbackNotificationService)
        {
            _feedbackNotificationService = feedbackNotificationService;
        }

        [HttpGet("check-notifications/{learnerId}")]
        public async Task<ActionResult<ResponseDTO>> CheckNotifications(int learnerId)
        {
            var result = await _feedbackNotificationService.CheckLearnerFeedbackNotificationsAsync(learnerId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return Ok(result);
        }

        [HttpPost("complete-feedback")]
        public async Task<ActionResult<ResponseDTO>> CompleteFeedback([FromBody] CompleteFeedbackDTO model)
        {
            if (model.FeedbackId <= 0)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid feedback ID."
                });
            }

            var result = await _feedbackNotificationService.ProcessFeedbackCompletionAsync(
                model.FeedbackId,
                model.ContinueStudying);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("check-learning-progress")]
        public async Task<IActionResult> CheckLearningProgress()
        {
            var result = await _feedbackNotificationService.CheckAndUpdateLearnerProgressAsync();
            return Ok(result);
        }


        [HttpPost("run-auto-check")]
        //[Authorize(Roles = "Admin,Staff,Manager")]
        public async Task<ActionResult<ResponseDTO>> RunAutomaticCheck()
        {
            var result = await _feedbackNotificationService.AutoCheckAndCreateFeedbackNotificationsAsync();

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("test-email-notification")]
        //[Authorize(Roles = "Admin,Staff,Manager")]
        public async Task<ActionResult<ResponseDTO>> TestEmailNotification([FromBody] TestEmailNotificationDTO model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Email is required."
                });
            }

            try
            {
                // Create a test email notification
                await _feedbackNotificationService.SendTestFeedbackEmailNotification(
                    model.Email,
                    model.LearnerName ?? "Test Learner",
                    model.FeedbackId,
                    model.TeacherName ?? "Test Teacher",
                    model.RemainingPayment
                );

                return Ok(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Test email notification sent to {model.Email}.",
                    Data = model
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to send test email: {ex.Message}"
                });
            }
        }
    }
}
