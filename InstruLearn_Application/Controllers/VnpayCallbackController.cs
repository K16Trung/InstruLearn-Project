using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Vnpay;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [ApiController]
    [Route("api/vnpay-callback")]
    public class VnpayCallbackController : ControllerBase
    {
        private readonly IVnpayService _vnpayService;
        private readonly IWalletService _walletService;
        private readonly ILogger<VnpayCallbackController> _logger;

        public VnpayCallbackController(
            IVnpayService vnpayService,
            IWalletService walletService,
            ILogger<VnpayCallbackController> logger)
        {
            _vnpayService = vnpayService;
            _walletService = walletService;
            _logger = logger;
        }

        [HttpGet]
        [Route("process")]
        public async Task<IActionResult> ProcessCallback()
        {
            _logger.LogInformation("Received VnPay callback request");
            _logger.LogInformation($"Query string: {Request.QueryString}");

            try
            {
                var response = _vnpayService.ProcessPaymentReturn(Request.Query);

                if (response.Success)
                {
                    _logger.LogInformation($"Processing successful payment for transaction {response.TransactionId}");
                    var result = await _walletService.ProcessVnpayReturnAsync(response);

                    if (result.IsSucceed)
                    {
                        _logger.LogInformation($"Payment processed successfully for transaction {response.TransactionId}");
                        return Redirect($"{Request.Scheme}://{Request.Host}/payment-success?txnId={response.TransactionId}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to process payment: {result.Message}");
                        return Redirect($"{Request.Scheme}://{Request.Host}/payment-failed?errorMessage={result.Message}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Payment was unsuccessful: {response.Message}");
                    return Redirect($"{Request.Scheme}://{Request.Host}/payment-failed?errorMessage={response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VnPay callback");
                return Redirect($"{Request.Scheme}://{Request.Host}/payment-failed?errorMessage=Internal server error");
            }
        }

        [HttpPost("ipn")]
        public async Task<IActionResult> InstantPaymentNotification()
        {
            _logger.LogInformation("Received VnPay IPN notification");

            try
            {
                var response = _vnpayService.ProcessPaymentReturn(Request.Query);

                if (response.Success)
                {
                    _logger.LogInformation($"Processing IPN for transaction {response.TransactionId}");
                    var result = await _walletService.ProcessVnpayReturnAsync(response);

                    if (result.IsSucceed)
                    {
                        _logger.LogInformation($"IPN processed successfully for transaction {response.TransactionId}");
                        return Ok(new { RspCode = "00", Message = "Confirm Success" });
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to process IPN: {result.Message}");
                        return BadRequest(new { RspCode = "99", Message = result.Message });
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid IPN notification: {response.Message}");
                    return BadRequest(new { RspCode = "99", Message = response.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VnPay IPN notification");
                return StatusCode(500, new { RspCode = "99", Message = "Internal server error" });
            }
        }
    }
}
