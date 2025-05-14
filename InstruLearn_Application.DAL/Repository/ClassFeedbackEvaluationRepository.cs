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
    public class ClassFeedbackEvaluationRepository : GenericRepository<ClassFeedbackEvaluation>, IClassFeedbackEvaluationRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassFeedbackEvaluationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassFeedbackEvaluation>> GetEvaluationsByFeedbackIdAsync(int feedbackId)
        {
            return await _context.ClassFeedbackEvaluations
                .Include(e => e.Criterion)
                .Where(e => e.FeedbackId == feedbackId)
                .ToListAsync();
        }
    }
}
