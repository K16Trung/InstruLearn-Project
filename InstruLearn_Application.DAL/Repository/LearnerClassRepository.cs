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
    public class LearnerClassRepository : GenericRepository<Learner_class>, ILearnerClassRepository
    {
        private readonly ApplicationDbContext _context;

        public LearnerClassRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Learner_class> GetByLearnerAndClassAsync(int learnerId, int classId)
        {
            return await _context.Learner_Classes
                .FirstOrDefaultAsync(lc => lc.LearnerId == learnerId && lc.ClassId == classId);
        }

        public async Task<bool> RemoveByLearnerAndClassAsync(int learnerId, int classId)
        {
            var learnerClass = await GetByLearnerAndClassAsync(learnerId, classId);
            if (learnerClass == null)
                return false;

            _context.Learner_Classes.Remove(learnerClass);
            return true;

        }
    }
}
