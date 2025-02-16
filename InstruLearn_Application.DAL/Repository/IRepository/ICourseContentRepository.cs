using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ICourseContentRepository : IGenericRepository<Course_Content>
    {
        Task<IEnumerable<Course_Content>> GetAllWithContentAsync();
        Task<Course_Content> GetByIdWithContentAsync(int contentId);
    }
}
