using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearningPathSessionRepository : IGenericRepository<LearningPathSession>
    {
        Task<List<LearningPathSession>> GetByLearningRegisIdAsync(int learningRegisId);
        Task AddRangeAsync(IEnumerable<LearningPathSession> sessions);
        Task<LearningPathSession> GetBySessionNumberAsync(int learningRegisId, int sessionNumber);
        Task UpdateCompletionStatusAsync(int learningPathSessionId, bool isCompleted);
    }
}
