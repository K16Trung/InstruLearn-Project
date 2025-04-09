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
                string finalSuccessUrl = !string.IsNullOrEmpty(successUrl) ? successUrl : _vnpaySettings.SuccessUrl;
                string finalFailureUrl = !string.IsNullOrEmpty(failureUrl) ? failureUrl : _vnpaySettings.FailureUrl;

                if (string.IsNullOrEmpty(finalSuccessUrl))
                    finalSuccessUrl = "http://localhost:3000/profile?";

                if (string.IsNullOrEmpty(finalFailureUrl))
                    finalFailureUrl = "http://localhost:3000/profile?";

                var vnpayLib = new VnPayLibrary();
                var response = vnpayLib.GetFullResponseData(Request.Query, _vnpaySettings.HashSecret);

                Console.WriteLine($"Processing VNPay return: TxnRef={response.TxnRef}, ResponseCode={response.ResponseCode}");

                if (!response.Success)
                {
                    return Redirect(finalFailureUrl);
                }

                var result = await _walletService.ProcessVnpayReturnAsync(response);

                if (response.ResponseCode == "00" && result.IsSucceed)
                {
                    return Redirect(finalSuccessUrl);
                }
                else
                {
                    return Redirect(finalFailureUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in VnpayReturn: {ex.Message}");

                string emergencyFailureUrl = "http://localhost:3000/payment/failure";

                try
                {
                    if (!string.IsNullOrEmpty(_vnpaySettings.FailureUrl))
                        return Redirect(_vnpaySettings.FailureUrl);

                    string queryFailureUrl = Request.Query["failureUrl"].ToString();
                    if (!string.IsNullOrEmpty(queryFailureUrl))
                        return Redirect(queryFailureUrl);

                    return Redirect(emergencyFailureUrl);
                }
                catch
                {
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
