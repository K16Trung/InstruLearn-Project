using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .Include(pi => pi.CoursePackage)
                .ToListAsync();
        }

        public async Task<Purchase_Items> GetByIdAsync(int id)
        {
            return await _appDbContext.Purchase_Items
                .Include(pi => pi.CoursePackage)
                .FirstOrDefaultAsync(pi => pi.PurchaseItemId == id);
        }

        public async Task<IEnumerable<Purchase_Items>> GetPurchaseItemWithFullCourseDetailsAsync()
        {
            return await _appDbContext.Purchase_Items
                .Include(pi => pi.CoursePackage)
                    .ThenInclude(cp => cp.Type)
                .Include(pi => pi.Purchase)
                .ToListAsync();
        }

        public async Task<Purchase_Items> GetPurchaseItemsWithFullCourseDetailsByPurchaseIdAsync(int purchaseItemId)
        {
            return await _appDbContext.Purchase_Items
                .Include(pi => pi.CoursePackage)
                    .ThenInclude(cp => cp.Type)
                .Include(pi => pi.Purchase)
                .FirstOrDefaultAsync(pi => pi.PurchaseItemId == purchaseItemId);
        }
    }
}