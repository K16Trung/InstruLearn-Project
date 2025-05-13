using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
                .Include(e => e.Teacher)
                .Include(e => e.Answers)
                    .ThenInclude(a => a.Question)
                .Include(e => e.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Include(e => e.LearningRegistration)
                    .ThenInclude(lr => lr.Learner)
                .ToListAsync();
        }

        public async Task<TeacherEvaluationFeedback> GetByIdWithDetailsAsync(int evaluationId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(e => e.Teacher)
                .Include(e => e.Answers)
                    .ThenInclude(a => a.Question)
                .Include(e => e.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Include(e => e.LearningRegistration)
                    .ThenInclude(lr => lr.Learner)
                .FirstOrDefaultAsync(e => e.EvaluationFeedbackId == evaluationId);
        }

        public async Task<List<TeacherEvaluationFeedback>> GetByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(e => e.Teacher)
                .Include(e => e.LearningRegistration)
                    .ThenInclude(lr => lr.Learner)
                .Where(e => e.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<List<TeacherEvaluationFeedback>> GetByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(e => e.Teacher)
                .Include(e => e.LearningRegistration)
                .Where(e => e.LearnerId == learnerId)
                .ToListAsync();
        }

        public async Task<TeacherEvaluationFeedback> GetByLearningRegistrationIdAsync(int learningRegistrationId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .Include(e => e.Teacher)
                .Include(e => e.Answers)
                    .ThenInclude(a => a.Question)
                .Include(e => e.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Include(e => e.LearningRegistration)
                    .ThenInclude(lr => lr.Learner)
                .FirstOrDefaultAsync(e => e.LearningRegistrationId == learningRegistrationId);
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

        public async Task AddAnswerAsync(TeacherEvaluationAnswer answer)
        {
            await _appDbContext.TeacherEvaluationAnswers.AddAsync(answer);
        }

        public async Task AddQuestionAsync(TeacherEvaluationQuestion question)
        {
            await _appDbContext.TeacherEvaluationQuestions.AddAsync(question);
        }

        public async Task AddOptionAsync(TeacherEvaluationOption option)
        {
            await _appDbContext.TeacherEvaluationOptions.AddAsync(option);
        }

        public async Task UpdateQuestionAsync(TeacherEvaluationQuestion question)
        {
            _appDbContext.TeacherEvaluationQuestions.Update(question);
        }

        public async Task UpdateAsync(TeacherEvaluationFeedback evaluation)
        {
            _appDbContext.TeacherEvaluationFeedbacks.Update(evaluation);
        }

        public async Task DeleteAsync(int questionId)
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

        public async Task<bool> ExistsByLearningRegistrationIdAsync(int learningRegistrationId)
        {
            return await _appDbContext.TeacherEvaluationFeedbacks
                .AnyAsync(e => e.LearningRegistrationId == learningRegistrationId);
        }
    }
}