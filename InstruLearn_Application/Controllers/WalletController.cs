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
        public async Task<IActionResult> VnpayReturn()
        {
            try
            {
                // Extract success and failure URLs from query parameters
                string successUrl = Request.Query["successUrl"].ToString();
                string failureUrl = Request.Query["failureUrl"].ToString();

                // Use VnPayLibrary to get a properly mapped response
                var vnpayLib = new VnPayLibrary();
                var response = vnpayLib.GetFullResponseData(Request.Query, _vnpaySettings.HashSecret);

                // Log transaction information
                Console.WriteLine($"Processing VNPay return: TxnRef={response.TxnRef}, ResponseCode={response.ResponseCode}");

                if (!response.Success)
                {
                    // If signature validation fails, redirect to failure URL
                    if (!string.IsNullOrEmpty(failureUrl))
                    {
                        return Redirect(failureUrl);
                    }
                    return BadRequest(new { message = "Invalid VNPay signature" });
                }

                var result = await _walletService.ProcessVnpayReturnAsync(response);

                if (!result.IsSucceed)
                {
                    Console.WriteLine($"Failed to process payment: {result.Message}");

                    // If payment processing fails, redirect to failure URL
                    if (!string.IsNullOrEmpty(failureUrl))
                    {
                        return Redirect(failureUrl);
                    }
                    return BadRequest(result);
                }

                // Payment successful, redirect to success URL
                if (!string.IsNullOrEmpty(successUrl))
                {
                    return Redirect(successUrl);
                }

                // If no success URL is provided, return success response as JSON
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in VnpayReturn: {ex.Message}");

                // Extract failure URL from query params
                string failureUrl = Request.Query["failureUrl"].ToString();

                // If exception occurs, redirect to failure URL
                if (!string.IsNullOrEmpty(failureUrl))
                {
                    return Redirect(failureUrl);
                }

                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
