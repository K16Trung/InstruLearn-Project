using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearnerClassRepository : IGenericRepository<Learner_class>
    {
        Task<Learner_class> GetByLearnerAndClassAsync(int learnerId, int classId);
        Task<bool> RemoveByLearnerAndClassAsync(int learnerId, int classId);
    }
}
