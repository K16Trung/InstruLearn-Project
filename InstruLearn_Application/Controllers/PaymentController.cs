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
    }
}
