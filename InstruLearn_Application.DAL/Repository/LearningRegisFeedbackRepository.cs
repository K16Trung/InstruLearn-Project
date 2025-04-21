using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class LearningRegisFeedbackRepository : GenericRepository<LearningRegisFeedback>, ILearningRegisFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public LearningRegisFeedbackRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LearningRegisFeedback> GetFeedbackWithDetailsAsync(int feedbackId)
        {
            return await _context.LearningRegisFeedbacks
                .Include(f => f.Learner)
                .Include(f => f.LearningRegistration)
                    .ThenInclude(lr => lr.Teacher)
                        .ThenInclude(t => t.Account)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }

        public async Task<LearningRegisFeedback> GetFeedbackByRegistrationIdAsync(int learningRegistrationId)
        {
            return await _context.LearningRegisFeedbacks
                .Include(f => f.Learner)
                .Include(f => f.LearningRegistration)
                    .ThenInclude(lr => lr.Teacher)
                        .ThenInclude(t => t.Account)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(f => f.LearningRegistrationId == learningRegistrationId);
        }

        public async Task<List<LearningRegisFeedback>> GetFeedbacksByTeacherIdAsync(int teacherId)
        {
            return await _context.LearningRegisFeedbacks
                .Include(f => f.Learner)
                .Include(f => f.LearningRegistration)
                    .ThenInclude(lr => lr.Teacher)
                        .ThenInclude(t => t.Account)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Where(f => f.LearningRegistration.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<List<LearningRegisFeedback>> GetFeedbacksByLearnerIdAsync(int learnerId)
        {
            return await _context.LearningRegisFeedbacks
                .Include(f => f.Learner)
                .Include(f => f.LearningRegistration)
                    .ThenInclude(lr => lr.Teacher)
                        .ThenInclude(t => t.Account)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .Where(f => f.LearnerId == learnerId)
                .ToListAsync();
        }
    }
}
