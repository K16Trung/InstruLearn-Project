using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class LearningRegisRepository : GenericRepository<Learning_Registration>, ILearningRegisRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public LearningRegisRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Learning_Registration>> GetPendingRegistrationsAsync()
        {
            return await _appDbContext.Learning_Registrations
                .Where(x => x.Status == LearningRegis.Pending)  // Ensure "Pending" matches your status naming convention
                .Include(x => x.Learner)
                .Include(x => x.Teacher)
                .Include(x => x.Learning_Registration_Type)
                .Include(x => x.Major)
                .Include(x => x.LearningRegistrationDay)
                .ToListAsync();
        }

        // ✅ Get pending registrations by LearnerId
        public async Task<IEnumerable<Learning_Registration>> GetRegistrationsByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Learning_Registrations
                .Where(x => x.LearnerId == learnerId)
                .Include(x => x.Learner)
                .Include(x => x.Teacher)
                .Include(x => x.Learning_Registration_Type)
                .Include(x => x.Major)
                .Include(x => x.LearningRegistrationDay)
                .ToListAsync();
        }

        public async Task<IEnumerable<Learning_Registration>> GetAllAsync()
        {
            return await _appDbContext.Learning_Registrations
                .Include(l => l.Teacher)
                .Include(l => l.Learner)
                .Include(l => l.Learning_Registration_Type)
                .Include(l => l.Major)
                .Include(l => l.LearningRegistrationDay)
                .ToListAsync();
        }

        public async Task<Learning_Registration> GetByIdAsync(int id)
        {
            return await _appDbContext.Learning_Registrations
                .Include(l => l.Teacher)
                .Include(l => l.Learner)
                .Include(l => l.Learning_Registration_Type)
                .Include(l => l.Major)
                .Include(l => l.LearningRegistrationDay)
                .FirstOrDefaultAsync(l => l.LearningRegisId == id);
        }
    }
}
