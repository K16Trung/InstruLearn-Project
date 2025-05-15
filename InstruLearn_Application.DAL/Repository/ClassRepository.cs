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
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .ToListAsync();
        }

        // Override GetByIdAsync to include navigation properties
        public async Task<Class> GetByIdAsync(int classId)
        {
            var classEntity = await _appDbContext.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity != null)
            {
                // Explicit loading for Teacher if it's still null
                if (classEntity.Teacher == null && classEntity.TeacherId > 0)
                {
                    Console.WriteLine($"Teacher not loaded with Include. Attempting explicit loading for TeacherId: {classEntity.TeacherId}");

                    // Load Teacher explicitly
                    var teacher = await _appDbContext.Teachers
                        .FirstOrDefaultAsync(t => t.TeacherId == classEntity.TeacherId);

                    if (teacher != null)
                    {
                        classEntity.Teacher = teacher;
                        Console.WriteLine($"Teacher loaded explicitly: {teacher.Fullname}");
                    }
                }

                // Explicit loading for Major if it's still null
                if (classEntity.Major == null && classEntity.MajorId > 0)
                {
                    Console.WriteLine($"Major not loaded with Include. Attempting explicit loading for MajorId: {classEntity.MajorId}");

                    // Load Major explicitly
                    var major = await _appDbContext.Majors
                        .FirstOrDefaultAsync(m => m.MajorId == classEntity.MajorId);

                    if (major != null)
                    {
                        classEntity.Major = major;
                        Console.WriteLine($"Major loaded explicitly: {major.MajorName}");
                    }
                }

                if (classEntity.LevelId.HasValue && classEntity.Level == null)
                {
                    // Try to load level explicitly if it failed with Include
                    var level = await _appDbContext.LevelAssigneds
                        .FirstOrDefaultAsync(l => l.LevelId == classEntity.LevelId.Value);

                    Console.WriteLine($"Explicit level load result: {level != null}");

                    if (level != null)
                    {
                        classEntity.Level = level;
                    }
                }
            }

            return classEntity;
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

        public async Task<List<Class>> GetClassesByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Major)
                .Include(c => c.Level)
                .Include(c => c.ClassDays)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }


    }
}
