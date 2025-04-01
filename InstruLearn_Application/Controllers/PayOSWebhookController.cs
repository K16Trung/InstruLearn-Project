using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.PayOSWebhook;
using InstruLearn_Application.Model.Models.DTO.Wallet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/webhook/payos")]
    [ApiController]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly IPayOSWebhookService _payOSWebhookService;
        private readonly IWalletService _walletService;

        public PayOSWebhookController(IPayOSWebhookService payOSWebhookService, IWalletService walletService)
        {
            _payOSWebhookService = payOSWebhookService;
            _walletService = walletService;
        }

        [HttpPost("payos-webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.OrderCode))
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            // Only process successful payments
            if (request.Status == "PAID")
            {
                var result = await _walletService.UpdatePaymentStatusAsync(request.OrderCode);
                if (!result.IsSucceed)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(new { message = "Payment status updated successfully" });
            }

            return Ok(new { message = "Webhook received" });
        }
    }
}
