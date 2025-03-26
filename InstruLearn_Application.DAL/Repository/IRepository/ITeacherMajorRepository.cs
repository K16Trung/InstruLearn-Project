using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ITeacherMajorRepository : IGenericRepository<TeacherMajor>
    {
        Task<bool> UpdateStatusAsync(int teacherMajorId, TeacherMajorStatus newStatus);

    }
}
