using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class TeacherEvaluationRepository : GenericRepository<TeacherEvaluationFeedback>, ITeacherEvaluationRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public TeacherEvaluationRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<TeacherEvaluationFeedback>> GetAllEvaluationsWithDetailsAsync()
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(t => t.Teacher)
                .Include(t => t.Learner)
                .Include(t => t.LearningRegistration)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.Question)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TeacherEvaluationFeedback> GetByIdWithDetailsAsync(int evaluationId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(t => t.Teacher)
                .Include(t => t.Learner)
                .Include(t => t.LearningRegistration)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.Question)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(t => t.EvaluationFeedbackId == evaluationId);
        }

        public async Task<TeacherEvaluationFeedback> GetByLearningRegistrationIdAsync(int learningRegistrationId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(t => t.Teacher)
                .Include(t => t.Learner)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.Question)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(t => t.LearningRegistrationId == learningRegistrationId);
        }

        public async Task<List<TeacherEvaluationFeedback>> GetByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(t => t.Learner)
                .Include(t => t.LearningRegistration)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.Question)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Where(t => t.TeacherId == teacherId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TeacherEvaluationFeedback>> GetByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(t => t.Teacher)
                .Include(t => t.LearningRegistration)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.Question)
                .Include(t => t.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Where(t => t.LearnerId == learnerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TeacherEvaluationFeedback>> GetPendingByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(t => t.Learner)
                .Include(t => t.LearningRegistration)
                .Where(t => t.TeacherId == teacherId &&
                       (t.Status == TeacherEvaluationStatus.NotStarted ||
                        t.Status == TeacherEvaluationStatus.InProgress))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsByLearningRegistrationIdAsync(int learningRegistrationId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .AnyAsync(t => t.LearningRegistrationId == learningRegistrationId);
        }

        public async Task<List<TeacherEvaluationQuestion>> GetActiveQuestionsWithOptionsAsync()
        {
            return await _appDbContext.TeacherEvaluationQuestions
                .Include(q => q.Options)
                .Where(q => q.IsActive)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        public async Task<TeacherEvaluationQuestion> GetQuestionWithOptionsAsync(int questionId)
        {
            return await _appDbContext.TeacherEvaluationQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.EvaluationQuestionId == questionId);
        }

        public async Task<List<TeacherEvaluationOption>> GetOptionsByQuestionIdAsync(int questionId)
        {
            return await _appDbContext.TeacherEvaluationOptions
                .Where(o => o.EvaluationQuestionId == questionId)
                .ToListAsync();
        }

        public async Task<List<TeacherEvaluationAnswer>> GetAnswersByFeedbackIdAsync(int evaluationFeedbackId)
        {
            return await _appDbContext.TeacherEvaluationAnswers
                .Include(a => a.Question)
                .Include(a => a.SelectedOption)
                .Where(a => a.EvaluationFeedbackId == evaluationFeedbackId)
                .ToListAsync();
        }

        public async Task AddAnswerAsync(TeacherEvaluationAnswer answer)
        {
            await _appDbContext.TeacherEvaluationAnswers.AddAsync(answer);
        }

        public async Task UpdateAnswerAsync(TeacherEvaluationAnswer answer)
        {
            _appDbContext.TeacherEvaluationAnswers.Update(answer);
        }

        public async Task AddQuestionAsync(TeacherEvaluationQuestion question)
        {
            await _appDbContext.TeacherEvaluationQuestions.AddAsync(question);
        }

        public async Task AddOptionAsync(TeacherEvaluationOption option)
        {
            await _appDbContext.TeacherEvaluationOptions.AddAsync(option);
        }

        public async Task<List<TeacherEvaluationQuestion>> GetAllQuestionsWithOptionsAsync()
        {
            return await _appDbContext.TeacherEvaluationQuestions
                .Include(q => q.Options)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        // New methods for update and delete operations
        public async Task UpdateQuestionAsync(TeacherEvaluationQuestion question)
        {
            _appDbContext.TeacherEvaluationQuestions.Update(question);
        }

        public async Task DeleteQuestionAsync(int questionId)
        {
            var question = await _appDbContext.TeacherEvaluationQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.EvaluationQuestionId == questionId);

            if (question != null)
            {
                if (question.Options != null && question.Options.Any())
                {
                    _appDbContext.TeacherEvaluationOptions.RemoveRange(question.Options);
                }

                _appDbContext.TeacherEvaluationQuestions.Remove(question);
            }
        }

        public async Task UpdateFeedbackAsync(TeacherEvaluationFeedback feedback)
        {
            _appDbContext.TeacherEvaluationFeedbacks.Update(feedback);
        }

        public async Task DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _appDbContext.TeacherEvaluationFeedbacks
                .Include(f => f.Answers)
                .FirstOrDefaultAsync(f => f.EvaluationFeedbackId == feedbackId);

            if (feedback != null)
            {
                // First, delete related answers
                if (feedback.Answers != null && feedback.Answers.Any())
                {
                    _appDbContext.TeacherEvaluationAnswers.RemoveRange(feedback.Answers);
                }

                // Delete the feedback
                _appDbContext.TeacherEvaluationFeedbacks.Remove(feedback);
            }
        }

        public async Task DeleteOptionAsync(int optionId)
        {
            var option = await _appDbContext.TeacherEvaluationOptions
                .FirstOrDefaultAsync(o => o.EvaluationOptionId == optionId);

            if (option != null)
            {
                _appDbContext.TeacherEvaluationOptions.Remove(option);
            }
        }

        public async Task UpdateOptionAsync(TeacherEvaluationOption option)
        {
            _appDbContext.TeacherEvaluationOptions.Update(option);
        }
    }
}