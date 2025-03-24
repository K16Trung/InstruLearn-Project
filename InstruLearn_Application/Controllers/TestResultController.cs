using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Test_Result;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultController : ControllerBase
    {
        private readonly ITestResultService _testResultService;

        public TestResultController(ITestResultService testResultService)
        {
            _testResultService = testResultService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTestResults()
        {
            var result = await _testResultService.GetAllTestResultsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTestResultById(int id)
        {
            var result = await _testResultService.GetTestResultByIdAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("learningRegis/{learningRegisId}/test-results")]
        public async Task<IActionResult> GetTestResultsByLearningRegisId(int learningRegisId)
        {
            var response = await _testResultService.GetTestResultsByLearningRegisIdAsync(learningRegisId);
            if (!response.IsSucceed)
            {
                return NotFound(response.Message);
            }

            return Ok(response.Data);
        }

        [HttpGet("learner/{learnerId}/test-results")]
        public async Task<IActionResult> GetTestResultsByLearnerId(int learnerId)
        {
            var response = await _testResultService.GetTestResultsByLearnerIdAsync(learnerId);
            if (!response.IsSucceed)
            {
                return NotFound(response.Message);
            }

            return Ok(response.Data);
        }



        [HttpPost("create")]
        public async Task<IActionResult> CreateTestResult([FromBody] CreateTestResultDTO createTestResultDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _testResultService.CreateTestResultAsync(createTestResultDTO);
            if (!result.IsSucceed)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetTestResultById), new { id = ((TestResultDTO)result.Data).TestResultId }, result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTestResult(int id)
        {
            var result = await _testResultService.DeleteTestResultAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}