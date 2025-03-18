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
    public class PurchaseRepository : GenericRepository<Purchase>, IPurchaseRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public PurchaseRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync()
        {
            return await _appDbContext.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.CoursePackage)
                    .ThenInclude(cp => cp.Type)
                .ToListAsync();
        }

        public async Task<Purchase> GetByIdAsync(int id)
        {
            return await _appDbContext.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.CoursePackage)
                    .ThenInclude(cp => cp.Type)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);
        }
    }
}
