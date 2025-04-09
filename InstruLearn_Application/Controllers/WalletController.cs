using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Configuration;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Payment;
using InstruLearn_Application.Model.Models.DTO.Vnpay;
using InstruLearn_Application.Model.Models.DTO.Wallet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IVnpayService _vnpayService;
        private readonly VnpaySettings _vnpaySettings;

        public WalletController(IWalletService walletService, IVnpayService vnpayService, VnpaySettings vnpaySettings)
        {
            _walletService = walletService;
            _vnpayService = vnpayService;
            _vnpaySettings = vnpaySettings;
        }

        [HttpPost("add-funds")]
        public async Task<IActionResult> AddFunds([FromBody] AddFundsRequest request)
        {
            var result = await _walletService.AddFundsToWallet(request.LearnerId, request.Amount);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("add-funds-vnpay")]
        public async Task<IActionResult> AddFundsWithVnpay([FromBody] AddFundsRequest request)
        {
            // Get the client IP address
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var result = await _walletService.AddFundsWithVnpay(request.LearnerId, request.Amount, ipAddress);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{learnerId}")]
        public async Task<IActionResult> GetWalletByLearnerId(int learnerId)
        {
            var response = await _walletService.GetWalletByLearnerIdAsync(learnerId);

            if (!response.IsSucceed)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("update-payment-status-by-ordercode")]
        public async Task<IActionResult> UpdatePaymentStatusByOrderCode([FromBody] PaymentStatusRequest request)
        {
            if (request == null || request.OrderCode <= 0)
            {
                return BadRequest(new { message = "Invalid OrderCode parameter" });
            }

            if (string.IsNullOrEmpty(request.Status))
            {
                request.Status = "PAID"; // Default to PAID if not specified
            }

            try
            {
                var result = await _walletService.UpdatePaymentStatusByOrderCodeAsync(request.OrderCode, request.Status);

                if (!result.IsSucceed)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the payment" });
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnpayReturn([FromQuery] string successUrl = null, [FromQuery] string failureUrl = null)
        {
            try
            {
                // Get the URLs from query parameters if provided, otherwise use the ones from settings
                string finalSuccessUrl = !string.IsNullOrEmpty(successUrl) ? successUrl : _vnpaySettings.SuccessUrl;
                string finalFailureUrl = !string.IsNullOrEmpty(failureUrl) ? failureUrl : _vnpaySettings.FailureUrl;

                // Fallback to application URLs if still empty
                if (string.IsNullOrEmpty(finalSuccessUrl))
                    finalSuccessUrl = "https://firebasestorage.googleapis.com/v0/b/sdn-project-aba8a.appspot.com/o/Screenshot%202025-04-02%20182541.png?alt=media&token=94a3f55f-2b3f-4d07-8153-4ffa4e8eed6e "; // Default frontend success page

                if (string.IsNullOrEmpty(finalFailureUrl))
                    finalFailureUrl = "https://firebasestorage.googleapis.com/v0/b/sdn-project-aba8a.appspot.com/o/Screenshot%202025-04-08%20211829.png?alt=media&token=68c9e81b-c748-4fde-997f-2fcc26b1bff6 "; // Default frontend failure page

                // Use VnPayLibrary to get a properly mapped response
                var vnpayLib = new VnPayLibrary();
                var response = vnpayLib.GetFullResponseData(Request.Query, _vnpaySettings.HashSecret);

                // Log transaction information
                Console.WriteLine($"Processing VNPay return: TxnRef={response.TxnRef}, ResponseCode={response.ResponseCode}");

                if (!response.Success)
                {
                    // If signature validation fails, redirect to failure URL
                    return Redirect(finalFailureUrl);
                }

                var result = await _walletService.ProcessVnpayReturnAsync(response);

                if (response.ResponseCode == "00" && result.IsSucceed)
                {
                    // Payment successful, redirect to success URL
                    return Redirect(finalSuccessUrl);
                }
                else
                {
                    // Payment failed or processing error, redirect to failure URL
                    return Redirect(finalFailureUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in VnpayReturn: {ex.Message}");

                // Default failure URL if we can't get it from settings or query
                string emergencyFailureUrl = "http://localhost:3000/payment/failure";

                try
                {
                    // Try to use the failure URL from settings if available
                    if (!string.IsNullOrEmpty(_vnpaySettings.FailureUrl))
                        return Redirect(_vnpaySettings.FailureUrl);

                    // Try to get it from query params
                    string queryFailureUrl = Request.Query["failureUrl"].ToString();
                    if (!string.IsNullOrEmpty(queryFailureUrl))
                        return Redirect(queryFailureUrl);

                    // Use emergency fallback
                    return Redirect(emergencyFailureUrl);
                }
                catch
                {
                    // Last resort - return JSON response instead of redirect
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment processing failed",
                        error = ex.Message
                    });
                }
            }
        }
    }
}
