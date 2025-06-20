﻿using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningRegisController : ControllerBase
    {
        private readonly ILearningRegisService _learningRegisService;
        private readonly IPaymentInfoService _paymentInfoService;

        public LearningRegisController(ILearningRegisService learningRegisService, IPaymentInfoService paymentInfoService)
        {
            _learningRegisService = learningRegisService;
            _paymentInfoService = paymentInfoService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllLearningRegis()
        {
            var response = await _learningRegisService.GetAllLearningRegisAsync();

            var enrichedResponse = await _paymentInfoService.EnrichLearningRegisWithPaymentInfoAsync(response);

            return Ok(enrichedResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLearningRegisById(int id)
        {
            var response = await _learningRegisService.GetLearningRegisByIdAsync(id);

            var enrichedResponse = await _paymentInfoService.EnrichSingleLearningRegisWithPaymentInfoAsync(id, response);

            return Ok(enrichedResponse);
        }

        [HttpGet("LearningRegis/{teacherId}")]
        public async Task<IActionResult> GetLearningRegisByTeacherId(int teacherId)
        {
            var response = await _learningRegisService.GetRegistrationsByTeacherIdAsync(teacherId);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddLearningRegis([FromBody] CreateLearningRegisDTO createLearningRegisDTO)
        {
            var response = await _learningRegisService.CreateLearningRegisAsync(createLearningRegisDTO);
            return Ok(response);
        }

        [HttpPost("join-class")]
        public async Task<IActionResult> JoinClassWithWalletPayment([FromBody] LearnerClassPaymentDTO paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _learningRegisService.JoinClassWithWalletPaymentAsync(paymentDto);
            if (!response.IsSucceed)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLearningRegis(int id)
        {
            var response = await _learningRegisService.DeleteLearningRegisAsync(id);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetAllPendingRegistrations()
        {
            var result = await _learningRegisService.GetAllPendingRegistrationsAsync();
            return Ok(result);
        }

        [HttpGet("status/{learnerId}")]
        public async Task<IActionResult> GetRegistrationsByLearnerId(int learnerId)
        {
            var result = await _learningRegisService.GetRegistrationsByLearnerIdAsync(learnerId);
            var enrichedResult = await _paymentInfoService.EnrichLearningRegisWithPaymentInfoAsync(result);
            return Ok(enrichedResult);
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateLearningRegisStatus([FromBody] UpdateLearningRegisDTO updateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _learningRegisService.UpdateLearningRegisStatusAsync(updateDTO);

            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("reject/{learningRegisId}")]
        public async Task<IActionResult> RejectLearningRegistration(int learningRegisId, [FromBody] RejectLearningRegisDTO rejectDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _learningRegisService.RejectLearningRegisAsync(learningRegisId, rejectDTO.ResponseId);

            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("create-learning-path-sessions")]
        public async Task<IActionResult> CreateLearningPathSessions([FromBody] LearningPathSessionsCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _learningRegisService.CreateLearningPathSessionsAsync(createDTO);

            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}