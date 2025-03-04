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
    public class LearnerRepository : GenericRepository<Learner>, ILearnerRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public LearnerRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Learner?> GetByIdAsync(int leanerId)
        {
            return await _appDbContext.Learners
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.LearnerId == leanerId);
        }
    }
}
