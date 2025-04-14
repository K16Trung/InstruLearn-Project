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
    public class LearningPathSessionRepository : GenericRepository<LearningPathSession>, ILearningPathSessionRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public LearningPathSessionRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<LearningPathSession>> GetByLearningRegisIdAsync(int learningRegisId)
        {
            return await _appDbContext.LearningPathSessions
                .Where(x => x.LearningRegisId == learningRegisId)
                .OrderBy(x => x.SessionNumber)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<LearningPathSession> sessions)
        {
            await _appDbContext.LearningPathSessions.AddRangeAsync(sessions);
        }

        public async Task<LearningPathSession> GetBySessionNumberAsync(int learningRegisId, int sessionNumber)
        {
            return await _appDbContext.LearningPathSessions
                .FirstOrDefaultAsync(x => x.LearningRegisId == learningRegisId && x.SessionNumber == sessionNumber);
        }

        public async Task UpdateCompletionStatusAsync(int learningPathSessionId, bool isCompleted)
        {
            var session = await _appDbContext.LearningPathSessions.FindAsync(learningPathSessionId);
            if (session != null)
            {
                session.IsCompleted = isCompleted;
                _appDbContext.LearningPathSessions.Update(session);
            }
        }

    }
}
