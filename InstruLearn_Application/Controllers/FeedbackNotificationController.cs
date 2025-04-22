using InstruLearn_Application.BLL.Service.IService;
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

        [HttpGet("learner/{learnerId}")]
        public async Task<IActionResult> CheckLearnerFeedbackNotifications(int learnerId)
        {
            var response = await _feedbackNotificationService.CheckLearnerFeedbackNotificationsAsync(learnerId);
            return Ok(response);
        }
    }
}
