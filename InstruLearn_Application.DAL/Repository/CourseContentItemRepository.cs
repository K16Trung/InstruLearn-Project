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
    public class CourseContentItemRepository : GenericRepository<Course_Content_Item>, ICourseContentItemRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public CourseContentItemRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Course_Content_Item>> GetAllWithDetailsAsync()
        {
            return await _appDbContext.Course_Content_Items
                                     .Include(ci => ci.ItemType)
                                     .Include(ci => ci.CourseContent)
                                     .ToListAsync();
        }

        public async Task<Course_Content_Item> GetByIdWithDetailsAsync(int itemId)
        {
            return await _appDbContext.Course_Content_Items
                                     .Include(ci => ci.ItemType)
                                     .Include(ci => ci.CourseContent)
                                     .FirstOrDefaultAsync(ci => ci.ItemId == itemId);
        }

    }
}
