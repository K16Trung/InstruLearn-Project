using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationOption;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationQuestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ITeacherEvaluationService
    {
        Task<ResponseDTO> GetAllEvaluationsAsync();
        Task<ResponseDTO> GetEvaluationByIdAsync(int evaluationFeedbackId);
        Task<ResponseDTO> GetEvaluationByRegistrationIdAsync(int learningRegistrationId);
        Task<ResponseDTO> GetEvaluationsByTeacherIdAsync(int teacherId);
        Task<ResponseDTO> GetEvaluationsByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> GetPendingEvaluationsForTeacherAsync(int teacherId);
        Task<ResponseDTO> CreateQuestionWithOptionsAsync(CreateTeacherEvaluationQuestionDTO questionDTO);
        Task<ResponseDTO> UpdateQuestionAsync(int questionId, TeacherEvaluationQuestionDTO questionDTO);
        Task<ResponseDTO> DeleteQuestionAsync(int questionId);
        Task<ResponseDTO> CreateEvaluationAsync(int learningRegistrationId);
        Task<ResponseDTO> UpdateEvaluationFeedbackAsync(int evaluationFeedbackId, TeacherEvaluationDTO feedbackDTO);
        Task<ResponseDTO> DeleteEvaluationFeedbackAsync(int evaluationFeedbackId);
        Task<ResponseDTO> SubmitEvaluationAsync(SubmitTeacherEvaluationDTO submitDTO);
        Task<ResponseDTO> CheckAndCreateEvaluationRequestsAsync();
        Task<ResponseDTO> GetActiveQuestionsAsync();
    }
}
