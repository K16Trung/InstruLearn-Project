using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Payment;
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

        public WalletController(IWalletService walletService, IVnpayService vnpayService)
        {
            _walletService = walletService;
            _vnpayService = vnpayService;
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
    }
}
