using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class ClassFeedbackRepository : GenericRepository<ClassFeedback>, IClassFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassFeedbackRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ClassFeedback> GetFeedbackWithEvaluationsAsync(int feedbackId)
        {
            return await _context.ClassFeedbacks
                .Include(f => f.Class)
                .Include(f => f.Learner)
                .Include(f => f.Template)
                .Include(f => f.Evaluations)
                    .ThenInclude(e => e.Criterion)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }

        public async Task<IEnumerable<ClassFeedback>> GetFeedbacksByClassIdAsync(int classId)
        {
            return await _context.ClassFeedbacks
                .Include(f => f.Class)
                .Include(f => f.Learner)
                .Include(f => f.Template)
                .Include(f => f.Evaluations)
                    .ThenInclude(e => e.Criterion)
                .Where(f => f.ClassId == classId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassFeedback>> GetFeedbacksByLearnerIdAsync(int learnerId)
        {
            return await _context.ClassFeedbacks
                .Include(f => f.Class)
                .Include(f => f.Learner)
                .Include(f => f.Template)
                .Include(f => f.Evaluations)
                    .ThenInclude(e => e.Criterion)
                .Where(f => f.LearnerId == learnerId)
                .ToListAsync();
        }

        public async Task<ClassFeedback> GetFeedbackByClassAndLearnerAsync(int classId, int learnerId)
        {
            return await _context.ClassFeedbacks
                .Include(f => f.Class)
                .Include(f => f.Learner)
                .Include(f => f.Template)
                .Include(f => f.Evaluations)
                    .ThenInclude(e => e.Criterion)
                .FirstOrDefaultAsync(f => f.ClassId == classId && f.LearnerId == learnerId);
        }
        public async Task<ClassFeedback> GetAsync(Expression<Func<ClassFeedback, bool>> filter)
        {
            return await _context.ClassFeedbacks.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<ClassFeedback>> GetAllAsync(Expression<Func<ClassFeedback, bool>> filter = null)
        {
            IQueryable<ClassFeedback> query = _context.ClassFeedbacks;

            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync();
        }


    }
}
