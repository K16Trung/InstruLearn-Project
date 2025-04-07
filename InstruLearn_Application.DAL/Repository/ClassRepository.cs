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
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public ClassRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // Override GetAllAsync to include navigation properties
        public async Task<List<Class>> GetAllAsync()
        {
            return await _appDbContext.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major) 
                .Include(c => c.ClassDays)
                .ToListAsync();
        }

        // Override GetByIdAsync to include navigation properties
        public async Task<Class> GetByIdAsync(int classId)
        {
            return await _appDbContext.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.ClassDays)
                .FirstOrDefaultAsync(c => c.ClassId == classId);
        }

        public async Task<List<Class>> GetClassesByMajorIdAsync(int majorId)
        {
            return await _appDbContext.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.ClassDays)
                .Where(c => c.MajorId == majorId)
                .ToListAsync();
        }

    }
}
