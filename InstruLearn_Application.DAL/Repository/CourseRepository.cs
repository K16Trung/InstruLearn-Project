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
    public class CourseRepository : GenericRepository<Course_Package>, ICourseRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public CourseRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Course_Package>> GetAllAsync()
        {
            return await _appDbContext.CoursePackages
                .Include(c => c.Type)
                .ToListAsync();
        }

        public async Task<Course_Package> GetByIdAsync(int courseId)
        {
            return await _appDbContext.CoursePackages
                .Include(c => c.Type)
                .Include(c => c.CourseContents)
                    .ThenInclude(cc => cc.CourseContentItems)
                .Include(c => c.FeedBacks)
                    .ThenInclude(f => f.Account)
                .Include(c => c.FeedBacks)
                    .ThenInclude(f => f.FeedbackReplies)
                        .ThenInclude(fr => fr.Account)
                .Include(c => c.QnAs)
                    .ThenInclude(q => q.Account)
                .Include(c => c.QnAs)
                    .ThenInclude(q => q.QnAReplies)
                        .ThenInclude(qr => qr.Account)
                .FirstOrDefaultAsync(c => c.CoursePackageId == courseId);
        }
    }
}
