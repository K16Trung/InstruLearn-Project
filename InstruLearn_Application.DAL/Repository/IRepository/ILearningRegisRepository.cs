using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearningRegisRepository : IGenericRepository<Learning_Registration>
    {
        Task<IEnumerable<Learning_Registration>> GetPendingRegistrationsAsync();
        Task<IEnumerable<Learning_Registration>> GetRegistrationsByLearnerIdAsync(int learnerId);
        Task<IEnumerable<Learning_Registration>> GetAllAsync();
        Task<Learning_Registration> GetByIdAsync(int id);
        Task<Learning_Registration?> GetFirstOrDefaultAsync(
        Expression<Func<Learning_Registration, bool>> predicate,
        Func<IQueryable<Learning_Registration>, IIncludableQueryable<Learning_Registration, object>>? include = null);
    }
}
