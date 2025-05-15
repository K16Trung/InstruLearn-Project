using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public new async Task<List<Class>> GetAllAsync()
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .ToListAsync();
        }

        public new async Task<Class> GetByIdAsync(int id)
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .FirstOrDefaultAsync(c => c.ClassId == id);
        }

        public async Task<List<Class>> GetClassesByTeacherIdAsync(int teacherId)
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<List<Class>> GetClassesByMajorIdAsync(int majorId)
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .Where(c => c.MajorId == majorId)
                .ToListAsync();
        }

        public async Task<List<Class>> GetClassesByLevelIdAsync(int levelId)
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .Where(c => c.LevelId == levelId)
                .ToListAsync();
        }

        public async Task<List<Class>> GetAvailableClassesAsync()
        {
            return await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .Where(c => c.Status == ClassStatus.Scheduled || c.Status == ClassStatus.Ongoing)
                .ToListAsync();
        }

        public async Task<int> GetStudentCountAsync(int classId)
        {
            return await _context.Learner_Classes
                .Where(lc => lc.ClassId == classId)
                .CountAsync();
        }

        public async Task<bool> UpdateClassStatusAsync(int classId, ClassStatus newStatus)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
                return false;

            classEntity.Status = newStatus;
            _context.Classes.Update(classEntity);
            return await _context.SaveChangesAsync() > 0;
        }

        public new IQueryable<Class> GetQuery()
        {
            return _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .AsQueryable();
        }

        public async Task AddRangeAsync(IEnumerable<Class> classes)
        {
            await _context.Classes.AddRangeAsync(classes);
        }

        public void UpdateRange(IEnumerable<Class> classes)
        {
            _context.Classes.UpdateRange(classes);
        }

        public async Task DeleteRangeAsync(IEnumerable<int> classIds)
        {
            var classesToDelete = await _context.Classes
                .Where(c => classIds.Contains(c.ClassId))
                .ToListAsync();

            _context.Classes.RemoveRange(classesToDelete);
        }
    }
}