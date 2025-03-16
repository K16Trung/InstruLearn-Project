using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IFeedbackRepository : IGenericRepository<FeedBack>
    {
        Task<IEnumerable<FeedBack>> GetFeedbacksByCoursePackageIdAsync(int courseId);
    }
}
