using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public TeacherRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // Lấy tất cả giáo viên bao gồm thông tin về Major
        public async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await _appDbContext.Teachers
                .Include(t => t.TeacherMajors)
                .ThenInclude(tm => tm.Major)
                .ToListAsync();
        }

        // Lấy thông tin giáo viên theo ID bao gồm Major
        public async Task<Teacher> GetByIdAsync(int teacherId)
        {
            return await _appDbContext.Teachers
                .Include(t => t.TeacherMajors)
                .ThenInclude(tm => tm.Major)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }
    }
}