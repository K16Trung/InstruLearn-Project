using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationOption;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationQuestion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherEvaluationController : ControllerBase
    {
        private readonly ITeacherEvaluationService _teacherEvaluationService;

        public TeacherEvaluationController(ITeacherEvaluationService teacherEvaluationService)
        {
            _teacherEvaluationService = teacherEvaluationService;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ResponseDTO>> GetAll()
        {
            var result = await _teacherEvaluationService.GetAllEvaluationsAsync();
            return Ok(result);
        }

        [HttpGet("by-registration/{learningRegistrationId}")]
        public async Task<ActionResult<ResponseDTO>> GetByRegistrationId(int learningRegistrationId)
        {
            var result = await _teacherEvaluationService.GetEvaluationByRegistrationIdAsync(learningRegistrationId);
            return Ok(result);
        }

        [HttpGet("by-teacher/{teacherId}")]
        public async Task<ActionResult<ResponseDTO>> GetByTeacherId(int teacherId)
        {
            var result = await _teacherEvaluationService.GetEvaluationsByTeacherIdAsync(teacherId);
            return Ok(result);
        }

        [HttpGet("by-learner/{learnerId}")]
        public async Task<ActionResult<ResponseDTO>> GetByLearnerId(int learnerId)
        {
            var result = await _teacherEvaluationService.GetEvaluationsByLearnerIdAsync(learnerId);
            return Ok(result);
        }

        [HttpGet("questions/active")]
        public async Task<IActionResult> GetActiveQuestions()
        {
            var result = await _teacherEvaluationService.GetActiveQuestionsAsync();
            return result.IsSucceed ? Ok(result) : BadRequest(result);
        }

        [HttpGet("questions/{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var result = await _teacherEvaluationService.GetQuestionByIdAsync(questionId);
            return result.IsSucceed ? Ok(result) : BadRequest(result);
        }

        [HttpPost("create-questions")]
        public async Task<ActionResult<ResponseDTO>> CreateQuestion([FromBody] CreateTeacherEvaluationQuestionDTO questionDTO)
        {
            var result = await _teacherEvaluationService.CreateQuestionWithOptionsAsync(questionDTO);
            return Ok(result);
        }

        [HttpPut("update-question/{questionId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateQuestion(int questionId, [FromBody] UpdateTeacherEvaluationQuestionDTO questionDTO)
        {
            var result = await _teacherEvaluationService.UpdateQuestionAsync(questionId, questionDTO);
            return Ok(result);
        }

        [HttpDelete("delete-question/{questionId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteQuestion(int questionId)
        {
            var result = await _teacherEvaluationService.DeleteQuestionAsync(questionId);
            return Ok(result);
        }

        [HttpPut("questions/{questionId}/activate")]
        public async Task<IActionResult> ActivateQuestion(int questionId)
        {
            var result = await _teacherEvaluationService.ActivateQuestionAsync(questionId);
            return result.IsSucceed ? Ok(result) : BadRequest(result);
        }

        [HttpPut("questions/{questionId}/deactivate")]
        public async Task<IActionResult> DeactivateQuestion(int questionId)
        {
            var result = await _teacherEvaluationService.DeactivateQuestionAsync(questionId);
            return result.IsSucceed ? Ok(result) : BadRequest(result);
        }

        [HttpPost("submit-evaluation-feedback")]
        public async Task<ActionResult<ResponseDTO>> SubmitEvaluationFeedback([FromBody] SubmitTeacherEvaluationDTO submitDTO)
        {
            var result = await _teacherEvaluationService.SubmitEvaluationFeedbackAsync(submitDTO);
            return Ok(result);
        }

        [HttpGet("check-lastday-evaluations")]
        public async Task<IActionResult> CheckLastDayEvaluations()
        {
            var result = await _teacherEvaluationService.CheckForLastDayEvaluationsAsync();
            return Ok(result);
        }
    }
}