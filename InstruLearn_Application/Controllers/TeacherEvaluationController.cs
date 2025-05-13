using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationOption;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationQuestion;
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

        [HttpGet("{evaluationFeedbackId}")]
        public async Task<ActionResult<ResponseDTO>> GetById(int evaluationFeedbackId)
        {
            var result = await _teacherEvaluationService.GetEvaluationByIdAsync(evaluationFeedbackId);
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

        [HttpGet("pending/teacher/{teacherId}")]
        public async Task<ActionResult<ResponseDTO>> GetPendingForTeacher(int teacherId)
        {
            var result = await _teacherEvaluationService.GetPendingEvaluationsForTeacherAsync(teacherId);
            return Ok(result);
        }

        [HttpPost("create-questions")]
        public async Task<ActionResult<ResponseDTO>> CreateQuestion([FromBody] CreateTeacherEvaluationQuestionDTO questionDTO)
        {
            var result = await _teacherEvaluationService.CreateQuestionWithOptionsAsync(questionDTO);
            return Ok(result);
        }

        [HttpPut("update-question/{questionId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateQuestion(int questionId, [FromBody] TeacherEvaluationQuestionDTO questionDTO)
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

        [HttpPost("create-evaluation-feedback/{learningRegistrationId}")]
        public async Task<ActionResult<ResponseDTO>> Create(int learningRegistrationId)
        {
            var result = await _teacherEvaluationService.CreateEvaluationAsync(learningRegistrationId);
            return Ok(result);
        }

        [HttpPut("update-evaluation-feedback/{evaluationFeedbackId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateEvaluationFeedback(int evaluationFeedbackId, [FromBody] TeacherEvaluationDTO feedbackDTO)
        {
            var result = await _teacherEvaluationService.UpdateEvaluationFeedbackAsync(evaluationFeedbackId, feedbackDTO);
            return Ok(result);
        }

        [HttpDelete("delete-evaluation-feedback/{evaluationFeedbackId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteEvaluationFeedback(int evaluationFeedbackId)
        {
            var result = await _teacherEvaluationService.DeleteEvaluationFeedbackAsync(evaluationFeedbackId);
            return Ok(result);
        }

        [HttpPost("submit-evaluation-feedback")]
        public async Task<ActionResult<ResponseDTO>> Submit([FromBody] SubmitTeacherEvaluationDTO submitDTO)
        {
            var result = await _teacherEvaluationService.SubmitEvaluationAsync(submitDTO);
            return Ok(result);
        }

        [HttpPost("auto-create-requests")]
        public async Task<ActionResult<ResponseDTO>> AutoCreateRequests()
        {
            var result = await _teacherEvaluationService.CheckAndCreateEvaluationRequestsAsync();
            return Ok(result);
        }

        [HttpGet("questions/active")]
        public async Task<ActionResult<ResponseDTO>> GetActiveQuestions()
        {
            var result = await _teacherEvaluationService.GetActiveQuestionsAsync();
            return Ok(result);
        }
    }
}