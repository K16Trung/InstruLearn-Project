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
    public class SyllabusRepository : GenericRepository<Syllabus>, ISyllabusRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public SyllabusRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Syllabus> GetSyllabusByClassIdAsync(int classId)
        {
            return await _appDbContext.Classes
                .Where(c => c.ClassId == classId)
                .Select(c => c.Syllabus)
                .FirstOrDefaultAsync();
        }
    }
}
