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
    public class CourseContentRepository : GenericRepository<Course_Content>, ICourseContentRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public CourseContentRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Course_Content>> GetAllAsync()
        {
            return await _appDbContext.Course_Contents
                .Include(c => c.Course)
                .Include(c => c.CourseContentItems)
                .ToListAsync();
        }

        public async Task<Course_Content> GetByIdAsync(int contentId)
        {
            return await _appDbContext.Course_Contents
                .Include(c => c.Course)
                .Include(c => c.CourseContentItems)
                    .ThenInclude(ci => ci.ItemType)
                .FirstOrDefaultAsync(c => c.ContentId == contentId);
        }
    }
}
