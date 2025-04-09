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
    public class SyllabusContentRepository : GenericRepository<Syllabus_Content>, ISyllabusContentRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public SyllabusContentRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public new async Task<IEnumerable<Syllabus_Content>> GetAllAsync()
        {
            return await _appDbContext.Syllabus_Contents
                .Include(sc => sc.Syllabus)
                .ToListAsync();
        }

        public new async Task<Syllabus_Content?> GetByIdAsync(int id)
        {
            return await _appDbContext.Syllabus_Contents
                .Include(sc => sc.Syllabus)
                .FirstOrDefaultAsync(sc => sc.SyllabusContentId == id);
        }
    }
}