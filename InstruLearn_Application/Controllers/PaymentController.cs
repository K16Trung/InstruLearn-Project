using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Payment;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process-learning-payment")]
        public async Task<IActionResult> ProcessLearningPayment([FromBody] CreatePaymentDTO paymentDTO)
        {
            if (paymentDTO == null)
            {
                return BadRequest(new ResponseDTO { IsSucceed = false, Message = "Invalid payment data." });
            }

            var result = await _paymentService.ProcessLearningRegisPaymentAsync(paymentDTO);

            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("process-remaining-payment/{learningRegisId}")]
        public async Task<IActionResult> ProcessRemainingPayment(int learningRegisId)
        {
            var result = await _paymentService.ProcessRemainingPaymentAsync(learningRegisId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("reject-payment/{learningRegisId}")]
        public async Task<IActionResult> RejectPayment(int learningRegisId)
        {
            var result = await _paymentService.RejectPaymentAsync(learningRegisId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("class-initial-payments/{classId?}")]
        public async Task<IActionResult> GetClassInitialPayments(int? classId)
        {
            var result = await _paymentService.GetClassInitialPaymentsAsync(classId);

            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("confirm-class-remaining-payment")]
        public async Task<IActionResult> ConfirmClassRemainingPayment([FromBody] ClassRemainingPaymentDTO paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _paymentService.ConfirmClassRemainingPaymentAsync(
                paymentDto.LearnerId,
                paymentDto.ClassId);

            if (!response.IsSucceed)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("class-fully-paid-learners/{classId}")]
        public async Task<IActionResult> GetFullyPaidLearnersInClass(int classId)
        {
            var response = await _paymentService.GetFullyPaidLearnersInClassAsync(classId);
            return Ok(response);
        }

        [HttpGet("class-payment-status/{classId}")]
        public async Task<IActionResult> GetClassPaymentStatus(int classId)
        {
            var response = await _paymentService.GetClassPaymentStatusAsync(classId);

            if (response.IsSucceed)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

    }
}
