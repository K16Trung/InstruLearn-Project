using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IClassRepository : IGenericRepository<Class>
    {
        Task<List<Class>> GetAllAsync();
        Task<Class> GetByIdAsync(int classId);
        Task<List<Class>> GetClassesByMajorIdAsync(int majorId);
        Task<List<Class>> GetClassesByTeacherIdAsync(int teacherId);
        Task<List<ClassStudentDTO>> GetClassStudentsWithEligibilityAsync(int classId);

    }
}
