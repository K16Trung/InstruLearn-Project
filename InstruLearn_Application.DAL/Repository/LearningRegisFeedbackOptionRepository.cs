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
    public class LearningRegisFeedbackOptionRepository : GenericRepository<LearningRegisFeedbackOption>, ILearningRegisFeedbackOptionRepository
    {
        private readonly ApplicationDbContext _context;

        public LearningRegisFeedbackOptionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<LearningRegisFeedbackOption>> GetOptionsByQuestionIdAsync(int questionId)
        {
            return await _context.LearningRegisFeedbackOptions
                .Where(o => o.QuestionId == questionId)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }
    }
}
