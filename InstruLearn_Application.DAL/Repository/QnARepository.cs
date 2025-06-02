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
    public class QnARepository : GenericRepository<QnA>, IQnARepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public QnARepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<QnA>> GetAllAsync()
        {
            return await _appDbContext.QnA
                .Include(f => f.Account)
                .Include(f => f.QnAReplies)
                    .ThenInclude(r => r.Account)
                .ToListAsync();
        }
        public async Task<QnA> GetByIdAsync(int id)
        {
            return await _appDbContext.QnA
                .Include(f => f.Account)
                .Include(f => f.QnAReplies)
                    .ThenInclude(r => r.Account)
                .FirstOrDefaultAsync(f => f.QuestionId == id);
        }
    }
}
