using InstruLearn_Application.Model.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> GetQuery();
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(int id);
        Task<T> GettWithIncludesAsync(Expression<Func<T, bool>> filter, string includeProperties = "");
        /*Task<List<T>> GetAllWithIncludesAsync(Expression<Func<T, bool>> filter, string includeProperties = "");
        Task<List<T>> GetWithIncludesAsync(Expression<Func<T, bool>> filter, string includeProperties = "");*/
        Task AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(string id);
        Task<T> GetByIdAsync(string id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        Pagination<T> GetFilter(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null,
            string? foreignKey = null,
            object? foreignKeyId = null);
    }
}
