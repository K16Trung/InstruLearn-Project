using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnerNotificationController : ControllerBase
    {
        private readonly ILearnerNotificationService _learnerNotificationService;

        public LearnerNotificationController(ILearnerNotificationService learnerNotificationService)
        {
            _learnerNotificationService = learnerNotificationService;
        }

        [HttpGet("learner-notifications/{learnerId}")]
        public async Task<ActionResult<ResponseDTO>> GetLearnerEmailNotifications(int learnerId)
        {
            if (learnerId <= 0)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid learner ID."
                });
            }

            var result = await _learnerNotificationService.GetLearnerEmailNotificationsAsync(learnerId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("entrance-test-notifications/{learnerId}")]
        public async Task<ActionResult<ResponseDTO>> GetEntranceTestNotifications(int learnerId)
        {
            if (learnerId <= 0)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid learner ID."
                });
            }

            var result = await _learnerNotificationService.GetEntranceTestNotificationsAsync(learnerId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
