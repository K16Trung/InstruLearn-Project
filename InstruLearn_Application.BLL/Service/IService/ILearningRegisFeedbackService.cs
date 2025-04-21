using InstruLearn_Application.Model.Models.DTO.FeedbackSummary;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackQuestion;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILearningRegisFeedbackService
    {
        // Question management
        Task<ResponseDTO> CreateQuestionAsync(LearningRegisFeedbackQuestionDTO questionDTO);
        Task<ResponseDTO> UpdateQuestionAsync(int questionId, LearningRegisFeedbackQuestionDTO questionDTO);
        Task<ResponseDTO> DeleteQuestionAsync(int questionId);
        Task<ResponseDTO> ActivateQuestionAsync(int questionId);
        Task<ResponseDTO> DeactivateQuestionAsync(int questionId);
        Task<LearningRegisFeedbackQuestionDTO> GetQuestionAsync(int questionId);
        Task<List<LearningRegisFeedbackQuestionDTO>> GetAllActiveQuestionsAsync();

        // Feedback submission
        Task<ResponseDTO> SubmitFeedbackAsync(CreateLearningRegisFeedbackDTO createDTO);
        Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateLearningRegisFeedbackDTO updateDTO);
        Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId);
        Task<LearningRegisFeedbackDTO> GetFeedbackByIdAsync(int feedbackId);
        Task<LearningRegisFeedbackDTO> GetFeedbackByRegistrationIdAsync(int registrationId);
        Task<List<LearningRegisFeedbackDTO>> GetFeedbacksByTeacherIdAsync(int teacherId);
        Task<List<LearningRegisFeedbackDTO>> GetFeedbacksByLearnerIdAsync(string learnerId);

        // Analytics
        Task<TeacherFeedbackSummaryDTO> GetTeacherFeedbackSummaryAsync(int teacherId);
    }
}
