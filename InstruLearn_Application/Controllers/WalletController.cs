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

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost("add-funds")]
        public async Task<IActionResult> AddFunds([FromBody] AddFundsRequest request)
        {
            var result = await _walletService.AddFundsToWallet(request.LearnerId, request.Amount);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("update-payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus([FromBody] PaymentStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.OrderCode))
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            var result = await _walletService.UpdatePaymentStatusAsync(request.OrderCode);

            if (!result.IsSucceed)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Payment status updated successfully" });
        }
        
        [HttpPost("update-fail-payment-status")]
        public async Task<IActionResult> FailedPaymentStatus([FromBody] PaymentStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.OrderCode))
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            var result = await _walletService.FailPaymentAsync(request.OrderCode);

            if (!result.IsSucceed)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Payment status updated successfully" });
        }

        [HttpGet("{learnerId}")]
        public async Task<IActionResult> GetWalletByLearnerId(int learnerId)
        {
            var response = await _walletService.GetWalletByLearnerIdAsync(learnerId);

            if (!response.IsSucceed)
                return NotFound(response);

            return Ok(response);
        }

    }
}
