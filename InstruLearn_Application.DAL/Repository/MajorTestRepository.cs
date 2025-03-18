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
    public class MajorTestRepository : GenericRepository<MajorTest>, IMajorTestRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public MajorTestRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<MajorTest>> GetMajorTestsByMajorIdAsync(int majorId)
        {
            return await _appDbContext.MajorTests
                .Where(mt => mt.MajorId == majorId)
                .ToListAsync();
        }

    }
}
