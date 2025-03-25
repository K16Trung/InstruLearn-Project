using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IScheduleRepository : IGenericRepository<Schedules>
    {
        Task AddRangeAsync(IEnumerable<Schedules> schedules);
        Task<List<Schedules>> GetSchedulesByLearningRegisIdAsync(int learningRegisId);
        Task<List<Schedules>> GetSchedulesByLearnerAsync(int learnerId);
        Task<List<Schedules>> GetSchedulesByTeacherAsync(int teacherId);
    }
}
