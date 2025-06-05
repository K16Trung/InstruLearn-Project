using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class TeacherMajorRepository : GenericRepository<TeacherMajor>, ITeacherMajorRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public TeacherMajorRepository(ApplicationDbContext Appdbcontext) : base(Appdbcontext)
        {
            _appDbContext = Appdbcontext;
        }
        public async Task<IEnumerable<TeacherMajor>> GetAllAsync()
        {
            return await _appDbContext.TeacherMajors
                .Include(t => t.Teacher)
                    .ThenInclude(t => t.Account)
                .Include(m => m.Major)
                .ToListAsync();
        }
        public async Task<TeacherMajor> GetByIdAsync(int id)
        {
            return await _appDbContext.Set<TeacherMajor>()
                .Include(t => t.Teacher)
                    .ThenInclude(t => t.Account)
                .Include(m => m.Major)
                .FirstOrDefaultAsync(x => x.TeacherMajorId == id);
        }

        public async Task<bool> UpdateStatusAsync(int teacherMajorId, TeacherMajorStatus newStatus)
        {
            var teacherMajor = await _appDbContext.TeacherMajors
                .FirstOrDefaultAsync(tm => tm.TeacherMajorId == teacherMajorId);

            if (teacherMajor == null)
            {
                return false;
            }

            teacherMajor.Status = newStatus;

            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
