using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.PayOSWebhook;
using InstruLearn_Application.Model.Models.DTO.Wallet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/payos")]
    [ApiController]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly IPayOSWebhookService _payOSWebhookService;
        private readonly IWalletService _walletService;
        private readonly ILogger<PayOSWebhookController> _logger;

        public PayOSWebhookController(IPayOSWebhookService payOSWebhookService, IWalletService walletService, ILogger<PayOSWebhookController> logger)
        {
            _payOSWebhookService = payOSWebhookService;
            _walletService = walletService;
            _logger = logger;
        }

        [HttpGet("result")]
        public async Task<IActionResult> PaymentResult(
    [FromQuery] string id,
    [FromQuery] string cancel,
    [FromQuery] string code = "",
    [FromQuery] string status = "",
    [FromQuery] string orderCode = "")
        {
            _logger.LogInformation($"Received PayOS result callback: id={id}, cancel={cancel}, code={code}, status={status}, orderCode={orderCode}");

            try
            {
                // Check if this is a cancel request or payment success
                if (cancel == "true")
                {
                    await _walletService.FailPaymentAsync(id);
                    // Redirect to your frontend
                    return Redirect("https://www.facebook.com/FPTU.HCM/payment-failed");
                }

                // For successful payments, we'll receive code=00 and status=PAID
                if (code == "00" && status == "PAID")
                {
                    var result = await _walletService.UpdatePaymentStatusAsync(id);

                    if (result.IsSucceed)
                    {
                        _logger.LogInformation($"Successfully updated payment for transaction {id}");
                        // Redirect to your frontend
                        return Redirect("https://fap.fpt.edu.vn/payment-success");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to update payment: {result.Message}");
                        // Redirect to your frontend
                        return Redirect($"https://www.facebook.com/FPTU.HCM/payment-failed?message={Uri.EscapeDataString(result.Message)}");
                    }
                }

                // Default case for other statuses
                await _walletService.FailPaymentAsync(id);
                // Redirect to your frontend
                return Redirect("https://www.facebook.com/FPTU.HCM/payment-failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS payment result");
                // Redirect to your frontend with error message
                return Redirect("https://www.facebook.com/FPTU.HCM/payment-failed?message=An+error+occurred");
            }
        }



        [HttpPost("webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookRequest request)
        {
            _logger.LogInformation("Received PayOS webhook");

            try
            {
                if (request?.Data == null)
                {
                    _logger.LogWarning("Invalid webhook data received");
                    return BadRequest(new { message = "Invalid webhook data" });
                }

                string transactionId = request.Data.TransactionId;

                // Process based on payment status
                if (request.Data.Status == "PAID")
                {
                    var result = await _walletService.UpdatePaymentStatusAsync(transactionId);

                    if (result.IsSucceed)
                    {
                        _logger.LogInformation($"Successfully processed webhook payment for transaction {transactionId}");
                        return Ok(new { message = "Payment processed successfully" });
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to process payment: {result.Message}");
                        return BadRequest(new { message = result.Message });
                    }
                }
                else if (request.Data.Status == "CANCELLED" || request.Data.Status == "FAILED")
                {
                    var result = await _walletService.FailPaymentAsync(transactionId);

                    if (result.IsSucceed)
                    {
                        _logger.LogInformation($"Payment marked as failed for transaction {transactionId}");
                        return Ok(new { message = "Payment marked as failed" });
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to mark payment as failed: {result.Message}");
                        return BadRequest(new { message = result.Message });
                    }
                }

                return Ok(new { message = "Webhook received but no action taken" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
