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
    public class CertificationRepository : GenericRepository<Certification>, ICertificationRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public CertificationRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            {
                _appDbContext = appDbContext;
            }
        }

        public async Task<IEnumerable<Certification>> GetAllAsync()
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                    .ThenInclude(l => l.Account)
                .Include(c => c.CoursePackages)
                .ToListAsync();
        }

        public async Task<Certification> GetByIdAsync(int id)
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                    .ThenInclude(l => l.Account)
                .Include(c => c.CoursePackages)
                .FirstOrDefaultAsync(c => c.CertificationId == id);
        }
    }
}
