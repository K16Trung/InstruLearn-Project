using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IPurchaseItemRepository : IGenericRepository<Purchase_Items>
    {
        Task<IEnumerable<Purchase_Items>> GetPurchaseItemWithFullCourseDetailsAsync();

        Task<Purchase_Items> GetPurchaseItemsWithFullCourseDetailsByPurchaseIdAsync(int purchaseId);
    }
}
