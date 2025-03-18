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
    public class PurchaseItemRepository : GenericRepository<Purchase_Items>, IPurchaseItemRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public PurchaseItemRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<Purchase_Items>> GetAllAsync()
        {
            return await _appDbContext.Purchase_Items
                .Include(C => C.CoursePackage)
                .ToListAsync();
        }
        public async Task<Purchase_Items> GetByIdAsync(int id)
        {
            return await _appDbContext.Purchase_Items
                .Include(C => C.CoursePackage)
                .FirstOrDefaultAsync(f => f.CoursePackageId == id);
        }
    }
}
