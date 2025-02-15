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
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public CourseRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Course>> GetAllWithTypeAsync()
        {
            return await _appDbContext.Courses.Include(c => c.Type).ToListAsync();
        }

        public async Task<Course> GetByIdWithTypeAsync(int courseId)
        {
            return await _appDbContext.Courses.Include(c => c.Type)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }
    }
}
