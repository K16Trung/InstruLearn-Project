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
    public class SelfAssessmentRepository : GenericRepository<SelfAssessment>, ISelfAssessmentRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public SelfAssessmentRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<SelfAssessment>> GetAllActiveAsync()
        {
            return await _appDbContext.SelfAssessments
                .ToListAsync();
        }

        public async Task<SelfAssessment> GetByIdWithRegistrationsAsync(int id)
        {
            return await _appDbContext.SelfAssessments
                .Include(s => s.LearningRegistrations)
                    .ThenInclude(lr => lr.Learner)
                .Include(s => s.LearningRegistrations)
                    .ThenInclude(lr => lr.Teacher)
                .FirstOrDefaultAsync(s => s.SelfAssessmentId == id);
        }

    }
}
