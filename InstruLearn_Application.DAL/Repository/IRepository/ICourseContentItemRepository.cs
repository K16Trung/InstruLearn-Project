using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ICourseContentItemRepository : IGenericRepository<Course_Content_Item>
    {
        Task<IEnumerable<Course_Content_Item>> GetAllWithDetailsAsync();
        Task<Course_Content_Item> GetByIdWithDetailsAsync(int itemId);
    }
}
