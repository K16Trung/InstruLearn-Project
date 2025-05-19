using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ISelfAssessmentRepository : IGenericRepository<SelfAssessment>
    {
        Task<IEnumerable<SelfAssessment>> GetAllActiveAsync();
        Task<SelfAssessment> GetByIdWithRegistrationsAsync(int id);
    }
}
