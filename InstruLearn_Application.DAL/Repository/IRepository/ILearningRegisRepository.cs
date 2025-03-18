using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearningRegisRepository : IGenericRepository<Learning_Registration>
    {
        Task<IEnumerable<Learning_Registration>> GetPendingRegistrationsAsync();
        Task<IEnumerable<Learning_Registration>> GetPendingRegistrationsByLearnerIdAsync(int learnerId);
    }
}
