using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.UoW.IUoW
{
    public interface IUnitOfWork
    {
        ITeacherRepository TeacherRepository { get; }
        IAccountRepository AccountRepository { get; }
        ILearnerRepository LearnerRepository { get; }
        IAdminRepository AdminRepository { get; }
        IStaffRepository StaffRepository { get; }
        IManagerRepository ManagerRepository { get; }
        ICourseRepository CourseRepository { get; }
        ICourseTypeRepository CourseTypeRepository { get; }
        ApplicationDbContext dbContext { get; }
        public Task<int> SaveChangeAsync();
    }
}
