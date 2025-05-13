using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ITeacherEvaluationRepository : IGenericRepository<TeacherEvaluationFeedback>
    {
        Task<List<TeacherEvaluationFeedback>> GetAllEvaluationsWithDetailsAsync();
        Task<TeacherEvaluationFeedback> GetByIdWithDetailsAsync(int evaluationId);
        Task<List<TeacherEvaluationFeedback>> GetByTeacherIdAsync(int teacherId);
        Task<List<TeacherEvaluationFeedback>> GetByLearnerIdAsync(int learnerId);
        Task<List<TeacherEvaluationFeedback>> GetPendingByTeacherIdAsync(int teacherId);
        Task<TeacherEvaluationFeedback> GetByLearningRegistrationIdAsync(int learningRegistrationId);
        Task<bool> ExistsByLearningRegistrationIdAsync(int learningRegistrationId);
        Task<List<TeacherEvaluationQuestion>> GetActiveQuestionsWithOptionsAsync();
        Task<TeacherEvaluationQuestion> GetQuestionWithOptionsAsync(int questionId);
        Task<List<TeacherEvaluationOption>> GetOptionsByQuestionIdAsync(int questionId);
        Task AddAnswerAsync(TeacherEvaluationAnswer answer);
        Task<List<TeacherEvaluationAnswer>> GetAnswersByFeedbackIdAsync(int evaluationFeedbackId);
        Task<List<TeacherEvaluationQuestion>> GetAllQuestionsWithOptionsAsync();
        Task AddQuestionAsync(TeacherEvaluationQuestion question);
        Task AddOptionAsync(TeacherEvaluationOption option);
        Task UpdateQuestionAsync(TeacherEvaluationQuestion question);
        Task DeleteQuestionAsync(int questionId);
        Task UpdateFeedbackAsync(TeacherEvaluationFeedback feedback);
        Task DeleteFeedbackAsync(int feedbackId);
        Task DeleteOptionAsync(int optionId);
        Task UpdateOptionAsync(TeacherEvaluationOption option);
    }
}