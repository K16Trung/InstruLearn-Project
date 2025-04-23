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
    public class CertificationRepository : GenericRepository<Certification>, ICertificationRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public CertificationRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            {
                _appDbContext = appDbContext;
            }
        }

        public async Task<IEnumerable<Certification>> GetAllWithDetailsAsync()
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .Include(c => c.LearningRegistration)
                .ToListAsync();
        }

        public async Task<Certification> GetByIdWithDetailsAsync(int id)
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .Include(c => c.LearningRegistration)
                .FirstOrDefaultAsync(c => c.CertificationId == id);
        }

        public async Task<IEnumerable<Certification>> GetByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .Include(c => c.LearningRegistration)
                .Where(c => c.LearnerId == learnerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Certification>> GetByLearningRegisIdAsync(int learningRegisId)
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .Include(c => c.LearningRegistration)
                .Where(c => c.LearningRegisId == learningRegisId)
                .ToListAsync();
        }

        public async Task<bool> ExistsByLearningRegisIdAsync(int learningRegisId)
        {
            return await _appDbContext.Certifications.AnyAsync(c => c.LearningRegisId == learningRegisId);
        }
    }
}
