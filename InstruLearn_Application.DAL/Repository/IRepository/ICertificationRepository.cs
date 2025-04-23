using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ICertificationRepository : IGenericRepository<Certification>
    {
        Task<IEnumerable<Certification>> GetAllWithDetailsAsync();
        Task<Certification> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Certification>> GetByLearnerIdAsync(int learnerId);
        Task<IEnumerable<Certification>> GetByLearningRegisIdAsync(int learningRegisId);
        Task<bool> ExistsByLearningRegisIdAsync(int learningRegisId);
    }
}
