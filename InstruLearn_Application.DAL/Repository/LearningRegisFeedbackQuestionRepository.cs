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
    public class LearningRegisFeedbackQuestionRepository : GenericRepository<LearningRegisFeedbackQuestion>, ILearningRegisFeedbackQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public LearningRegisFeedbackQuestionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<LearningRegisFeedbackQuestion>> GetActiveQuestionsWithOptionsAsync()
        {
            return await _context.LearningRegisFeedbackQuestions
                .Include(q => q.Options.OrderBy(o => o.OptionId))
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        public async Task<LearningRegisFeedbackQuestion> GetQuestionWithOptionsAsync(int questionId)
        {
            return await _context.LearningRegisFeedbackQuestions
                .Include(q => q.Options.OrderBy(o => o.OptionId))
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }
    }
}
