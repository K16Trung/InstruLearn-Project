using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.PayOSWebhook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/webhook/payos")]
    [ApiController]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly IPayOSWebhookService _payOSWebhookService;

        public PayOSWebhookController(IPayOSWebhookService payOSWebhookService)
        {
            _payOSWebhookService = payOSWebhookService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] PayOSWebhookDTO webhookDto)
        {
            try
            {
                await _payOSWebhookService.ProcessWebhookAsync(webhookDto);
                return Ok(new { message = "Webhook processed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
