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
    public class LevelFeedbackCriterionRepository : GenericRepository<LevelFeedbackCriterion>, ILevelFeedbackCriterionRepository
    {
        private readonly ApplicationDbContext _context;

        public LevelFeedbackCriterionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LevelFeedbackCriterion>> GetCriteriaByTemplateIdAsync(int templateId)
        {
            return await _context.LevelFeedbackCriteria
                .Include(c => c.Template)
                .Where(c => c.TemplateId == templateId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

    }
}
