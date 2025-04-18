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
    public class LearnerCourseRepository : GenericRepository<Learner_Course>, ILearnerCourseRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public LearnerCourseRepository(ApplicationDbContext appDbcontext) : base(appDbcontext)
        {
            _appDbContext = appDbcontext;
        }

        public async Task<Learner_Course> GetByLearnerAndCourseAsync(int learnerId, int coursePackageId)
        {
            return await _appDbContext.LearnerCourses
                .FirstOrDefaultAsync(lc => lc.LearnerId == learnerId && lc.CoursePackageId == coursePackageId);
        }

        public async Task<List<Learner_Course>> GetByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.LearnerCourses
                .Include(lc => lc.CoursePackage)
                .Where(lc => lc.LearnerId == learnerId)
                .ToListAsync();
        }

        public async Task<List<Learner_Course>> GetByCoursePackageIdAsync(int coursePackageId)
        {
            return await _appDbContext.LearnerCourses
                .Include(lc => lc.Learner)
                .Where(lc => lc.CoursePackageId == coursePackageId)
                .ToListAsync();
        }

        public async Task<bool> UpdateProgressAsync(int learnerId, int coursePackageId, double percentage)
        {
            var learnerCourse = await GetByLearnerAndCourseAsync(learnerId, coursePackageId);

            if (learnerCourse == null)
            {
                // Create a new entry if it doesn't exist
                learnerCourse = new Learner_Course
                {
                    LearnerId = learnerId,
                    CoursePackageId = coursePackageId,
                    CompletionPercentage = percentage,
                    EnrollmentDate = DateTime.Now,
                    LastAccessDate = DateTime.Now
                };

                await _appDbContext.LearnerCourses.AddAsync(learnerCourse);
                await _appDbContext.SaveChangesAsync();
                return true;
            }

            // Update existing entry
            learnerCourse.CompletionPercentage = percentage;
            learnerCourse.LastAccessDate = DateTime.Now;

            _appDbContext.LearnerCourses.Update(learnerCourse);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
