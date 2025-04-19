using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearnerContentProgressRepository : IGenericRepository<Learner_Content_Progress>
    {
        Task<Learner_Content_Progress> GetByLearnerAndContentItemAsync(int learnerId, int contentItemId);
        Task<List<Learner_Content_Progress>> GetByLearnerAndCourseAsync(int learnerId, int coursePackageId);
        Task<bool> UpdateWatchTimeAsync(int learnerId, int contentItemId, double watchTimeInSeconds, bool isCompleted = false);
        Task<double> GetTotalWatchTimeForCourseAsync(int learnerId, int coursePackageId);
        Task<double> GetTotalVideoDurationForCourseAsync(int coursePackageId);
    }
}
