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
    public class LearningRegisFeedbackAnswerRepository : GenericRepository<LearningRegisFeedbackAnswer>, ILearningRegisFeedbackAnswerRepository
    {
        private readonly ApplicationDbContext _context;

        public LearningRegisFeedbackAnswerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<LearningRegisFeedbackAnswer>> GetAnswersByFeedbackIdAsync(int feedbackId)
        {
            return await _context.LearningRegisFeedbackAnswers
                .Include(a => a.Question)
                .Include(a => a.SelectedOption)
                .Where(a => a.FeedbackId == feedbackId)
                .ToListAsync();
        }
    }
}
