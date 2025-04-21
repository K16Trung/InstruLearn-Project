using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearnerCourseRepository : IGenericRepository<Learner_Course>
    {
        Task<Learner_Course> GetByLearnerAndCourseAsync(int learnerId, int coursePackageId);
        Task<List<Learner_Course>> GetByLearnerIdAsync(int learnerId);
        Task<List<Learner_Course>> GetByCoursePackageIdAsync(int coursePackageId);
        Task<bool> UpdateProgressAsync(int learnerId, int coursePackageId, double percentage);
        Task RecalculateProgressForAllLearnersInCourseAsync(int coursePackageId);
    }
}