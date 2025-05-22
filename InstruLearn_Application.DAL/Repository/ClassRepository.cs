using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Class;
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

        public async Task<List<ClassStudentDTO>> GetClassStudentsWithEligibilityAsync(int classId)
        {
            var studentsWithAccounts = await _context.Learner_Classes
                .Where(lc => lc.ClassId == classId)
                .Join(
                    _context.Learners,
                    lc => lc.LearnerId,
                    l => l.LearnerId,
                    (lc, l) => new { LearnerId = l.LearnerId, Learner = l }
                )
                .Join(
                    _context.Accounts,
                    join => join.Learner.AccountId,
                    a => a.AccountId,
                    (join, a) => new { join.LearnerId, join.Learner, Account = a }
                )
                .Select(joined => new {
                    joined.LearnerId,
                    joined.Learner.FullName,
                    joined.Account.Email,
                    joined.Account.PhoneNumber,
                    joined.Account.Avatar
                })
                .ToListAsync();

            var registrations = await _context.Learning_Registrations
                .Where(lr => lr.ClassId == classId)
                .Select(r => new {
                    r.LearnerId,
                    r.Status
                })
                .ToDictionaryAsync(r => r.LearnerId);

            var result = studentsWithAccounts.Select(s => new ClassStudentDTO
            {
                LearnerId = s.LearnerId,
                FullName = s.FullName ?? "N/A",
                Email = s.Email ?? "N/A",
                PhoneNumber = s.PhoneNumber ?? "N/A",
                Avatar = s.Avatar ?? "N/A",
                IsEligible = registrations.TryGetValue(s.LearnerId, out var registration) ?
                    (registration.Status == LearningRegis.Rejected ? false :
                     registration.Status == LearningRegis.Accepted ||
                     registration.Status == LearningRegis.FullyPaid ? true : null) : null,
            }).ToList();

            return result;
        }
    }
}