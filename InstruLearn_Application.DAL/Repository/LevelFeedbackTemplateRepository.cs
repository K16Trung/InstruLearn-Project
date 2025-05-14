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
    public class LevelFeedbackTemplateRepository : GenericRepository<LevelFeedbackTemplate>, ILevelFeedbackTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public LevelFeedbackTemplateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LevelFeedbackTemplate> GetTemplateWithCriteriaAsync(int templateId)
        {
            return await _context.LevelFeedbackTemplates
                .Include(t => t.Level)
                    .ThenInclude(l => l.Major)
                .Include(t => t.Criteria.OrderBy(c => c.DisplayOrder))
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);
        }

        public async Task<IEnumerable<LevelFeedbackTemplate>> GetAllTemplatesWithCriteriaAsync()
        {
            return await _context.LevelFeedbackTemplates
                .Include(t => t.Level)
                    .ThenInclude(l => l.Major)
                .Include(t => t.Criteria.OrderBy(c => c.DisplayOrder))
                .ToListAsync();
        }

        public async Task<LevelFeedbackTemplate> GetTemplateForLevelAsync(int levelId)
        {
            return await _context.LevelFeedbackTemplates
                .Include(t => t.Level)
                .Include(t => t.Criteria.OrderBy(c => c.DisplayOrder))
                .FirstOrDefaultAsync(t => t.LevelId == levelId && t.IsActive);
        }
        public async Task<LevelFeedbackTemplate> GetAsync(Expression<Func<LevelFeedbackTemplate, bool>> filter)
        {
            return await _context.LevelFeedbackTemplates.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<LevelFeedbackTemplate>> GetAllAsync(Expression<Func<LevelFeedbackTemplate, bool>> filter = null)
        {
            IQueryable<LevelFeedbackTemplate> query = _context.LevelFeedbackTemplates;

            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync();
        }

    }
}
