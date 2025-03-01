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
    public class QnARepliesRepository : GenericRepository<QnAReplies>, IQnARepliesRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public QnARepliesRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<QnAReplies>> GetAllAsync()
        {
            return await _appDbContext.QnAReplies
                .Include(f => f.Account)
                .ToListAsync();
        }
        public async Task<QnAReplies> GetByIdAsync(int qnaRepliesId)
        {
            return await _appDbContext.QnAReplies
                .Include(f => f.Account)
                .FirstOrDefaultAsync(f => f.QnARepliesId == qnaRepliesId);
        }
    }
}
