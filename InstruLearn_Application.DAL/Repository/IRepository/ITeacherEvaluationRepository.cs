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
        Task<TeacherEvaluationFeedback> GetByLearningRegistrationIdAsync(int learningRegistrationId);
        Task<bool> ExistsByLearningRegistrationIdAsync(int learningRegistrationId);
        Task<List<TeacherEvaluationQuestion>> GetActiveQuestionsWithOptionsAsync();
        Task<TeacherEvaluationQuestion> GetQuestionWithOptionsAsync(int questionId);
        Task AddAnswerAsync(TeacherEvaluationAnswer answer);
        Task AddQuestionAsync(TeacherEvaluationQuestion question);
        Task AddOptionAsync(TeacherEvaluationOption option);
        Task UpdateQuestionAsync(TeacherEvaluationQuestion question);
        Task UpdateOptionAsync(TeacherEvaluationOption option);
    }
}