using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackQuestion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningRegisFeedbackController : ControllerBase
    {
        private readonly ILearningRegisFeedbackService _feedbackService;

        public LearningRegisFeedbackController(ILearningRegisFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        #region Question Management

        [HttpGet("GetActiveQuestions")]
        public async Task<IActionResult> GetActiveQuestions()
        {
            var questions = await _feedbackService.GetAllActiveQuestionsAsync();
            return Ok(questions);
        }

        [HttpGet("questions/{questionId}")]
        public async Task<IActionResult> GetQuestion(int questionId)
        {
            var question = await _feedbackService.GetQuestionAsync(questionId);
            if (question == null)
                return NotFound(new { Message = "Không tìm thấy câu hỏi" });

            return Ok(question);
        }

        [HttpPost("CreateQuestion")]
        public async Task<IActionResult> CreateQuestion([FromBody] LearningRegisFeedbackQuestionDTO questionDTO)
        {
            var response = await _feedbackService.CreateQuestionAsync(questionDTO);
            return Ok(response);
        }

        [HttpPut("UpdateQuestion/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] LearningRegisFeedbackQuestionDTO questionDTO)
        {
            var response = await _feedbackService.UpdateQuestionAsync(questionId, questionDTO);
            return Ok(response);
        }

        [HttpDelete("DeleteQuestion/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var response = await _feedbackService.DeleteQuestionAsync(questionId);
            return Ok(response);
        }

        [HttpPut("questions/{questionId}/activate")]
        public async Task<IActionResult> ActivateQuestion(int questionId)
        {
            var response = await _feedbackService.ActivateQuestionAsync(questionId);
            return Ok(response);
        }

        [HttpPut("questions/{questionId}/deactivate")]
        public async Task<IActionResult> DeactivateQuestion(int questionId)
        {
            var response = await _feedbackService.DeactivateQuestionAsync(questionId);
            return Ok(response);
        }

        #endregion

        #region Feedback Submission

        [HttpPost("SubmitFeedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] CreateLearningRegisFeedbackDTO createDTO)
        {
            var response = await _feedbackService.SubmitFeedbackAsync(createDTO);
            return Ok(response);
        }

        [HttpGet("Feedback/{feedbackId}")]
        public async Task<IActionResult> GetFeedback(int feedbackId)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(feedbackId);
            if (feedback == null)
                return NotFound(new { Message = "Không tìm thấy đánh giá" });

            return Ok(feedback);
        }

        [HttpGet("registration/{registrationId}")]
        public async Task<IActionResult> GetFeedbackByRegistration(int registrationId)
        {
            var feedback = await _feedbackService.GetFeedbackByRegistrationIdAsync(registrationId);
            if (feedback == null)
                return NotFound(new { Message = "Không tìm thấy đánh giá cho đăng ký học này" });

            return Ok(feedback);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetFeedbacksByTeacher(int teacherId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByTeacherIdAsync(teacherId);
            return Ok(feedbacks);
        }

        [HttpGet("learner/{learnerId}")]
        public async Task<IActionResult> GetFeedbacksByLearner(int learnerId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByLearnerIdAsync(learnerId);
            return Ok(feedbacks);
        }

        [HttpPut("UpdateFeedback/{feedbackId}")]
        public async Task<IActionResult> UpdateFeedback(int feedbackId, [FromBody] UpdateLearningRegisFeedbackDTO updateDTO)
        {
            var response = await _feedbackService.UpdateFeedbackAsync(feedbackId, updateDTO);
            return Ok(response);
        }

        [HttpDelete("DeleteFeedback/{feedbackId}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            var response = await _feedbackService.DeleteFeedbackAsync(feedbackId);
            return Ok(response);
        }

        #endregion

        #region Analytics

        [HttpGet("analytics/teacher/{teacherId}")]
        public async Task<IActionResult> GetTeacherFeedbackSummary(int teacherId)
        {
            var summary = await _feedbackService.GetTeacherFeedbackSummaryAsync(teacherId);
            return Ok(summary);
        }

        #endregion
    }
}